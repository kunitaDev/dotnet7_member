using Microsoft.AspNetCore.Mvc;
using static dotnet7_member.Models.Accounts.AccountsModel;
using System.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Data.SqlClient;
using dotnet7_member.Services;
using dotnet7_member.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace dotnet7_member.Controllers
{
    public class AccountController : Controller
    {
        private readonly SqlConnectionContext _sqlConnectionContext;
        private readonly string _conn;

        public AccountController(SqlConnectionContext sqlConnectionContext)
        {
            _sqlConnectionContext = sqlConnectionContext;
            _conn = _sqlConnectionContext.GetConnectionString();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp() => User.Identity.IsAuthenticated ? RedirectToRoute(new { controller = "Home", action = "Index" }) : View();
        public IActionResult SignIn() => User.Identity.IsAuthenticated ? RedirectToRoute(new { controller = "Home", action = "Index" }) : View();

        public async Task<IActionResult> SubmitRegister(RegisterRequest registerRequest)
        {
            try
            {
                registerRequest.Password = CreatePasswordHash(registerRequest.Password);
                var (IsInsert, Msg) = await Register(registerRequest);
                if (IsInsert)
                {
                    return RedirectToRoute(new { controller = "Account", action = "SignIn" });
                }
                else if (Msg == "Duplicate user")
                {
                    ViewBag.Msg = "คุณป็นเคยเป็นสมาชิกแล้วกรุณาเข้าสู่ระบบ";
                    return View("Login");
                }
                else
                {
                    return View("Error", new ErrorViewModel { RequestId = "test error!!!" });
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        public async Task<(bool, string)> Register(RegisterRequest registerRequest)
        {
            bool Flag = false;
            string Message = "";
            try
            {
                List<LoginResponse> loginResponses = new();
                loginResponses = GetUserByEmail(registerRequest.Email);
                if (loginResponses.Count > 0)
                {
                    Flag = false;
                    Message = "Duplicate user";
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(_conn))
                    {
                        SqlCommand command = new SqlCommand("uspRegisterMember", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();

                        SqlParameter sp = new();
                        sp = new SqlParameter();
                        sp.ParameterName = "@USER_FIRSTNAME";
                        sp.Value = registerRequest.Firstname;
                        command.Parameters.Add(sp);

                        sp = new SqlParameter();
                        sp.ParameterName = "@USER_LASTNAME";
                        sp.Value = registerRequest.Lastname;
                        command.Parameters.Add(sp);

                        sp = new SqlParameter();
                        sp.ParameterName = "@USER_EMAIL";
                        sp.Value = registerRequest.Email;
                        command.Parameters.Add(sp);

                        string password = CreatePasswordHash(registerRequest.ConfirmPassword);
                        sp = new SqlParameter();
                        sp.ParameterName = "@USER_PASSWORD";
                        sp.Value = password;
                        command.Parameters.Add(sp);

                        int result = command.ExecuteNonQuery();
                        Flag = (result != 0) ? true : false;
                        connection.Close();
                    }

                }


                return (Flag, Message);
            }
            catch (Exception ex)
            {
                throw new(ex.Message);
            }
        }

        public string CreatePasswordHash(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hased = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));

            return $"{Convert.ToBase64String(salt)}.{hased}";
        }

        public async Task<(bool, string, LoginResponse)> Login(LoginRequest loginRequest)
        {
            try
            {
                List<LoginResponse> loginResponses = new();
                loginResponses = GetUserByEmail(loginRequest.Email);
                var User = loginResponses.SingleOrDefault(u => u.Email.ToLower() == loginRequest.Email.ToLower());
                if (loginResponses == null || loginResponses.Count == 0)
                {
                    return (false, "ไม่พบข้อมูลผู้ใช้งาน กรุณาสมัครสมาชิก", null);
                }

                if (VerifyPassword(User.Password, loginRequest.Password))
                {
                    return (true, "Login success.", User);
                }
                else
                {
                    return (false, "รหัสผ่านไม่ถูกต้อง", User);
                }

            }
            catch (Exception ex)
            {
                string msg = $"[Login], {ex.Message}";
                throw new(msg);
            }
        }

        public async Task<IActionResult> SubmitLogin(LoginRequest loginRequest)
        {
            try
            {
                var (IsLogin, Message, user) = await Login(loginRequest);
                if (IsLogin && Message == "Login success.")
                {
                    string Name = $"{user.FirstName} {user.LastName}";
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email,user.Email),
                        new Claim(ClaimTypes.Name,Name),
                        new Claim(ClaimTypes.Role, user.RoleName),
                        new Claim("Id", user.UserId.ToString())
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "Login");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Msg = Message;
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        private List<LoginResponse> GetUserByEmail(string Email)
        {
            try
            {
                List<LoginResponse> ListloginResponse = new();
                LoginResponse loginResponse = new();
                using (SqlConnection connection = new SqlConnection(_conn))
                {
                    SqlCommand command = new SqlCommand("uspGetUserByEmail", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    SqlDataReader reader = null;

                    SqlParameter sp = new();
                    sp = new SqlParameter();
                    sp.ParameterName = "@USER_EMAIL";
                    sp.Value = Email;
                    command.Parameters.Add(sp);

                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        loginResponse.UserId = (reader["USER_ID"] != DBNull.Value) ? int.Parse(reader["USER_ID"].ToString()) : 0;
                        loginResponse.FirstName = (reader["USER_FIRSTNAME"] != DBNull.Value) ? reader["USER_FIRSTNAME"].ToString() : "";
                        loginResponse.LastName = (reader["USER_LASTNAME"] != DBNull.Value) ? reader["USER_LASTNAME"].ToString() : "";
                        loginResponse.Email = (reader["USER_EMAIL"] != DBNull.Value) ? reader["USER_EMAIL"].ToString() : "";
                        loginResponse.Password = (reader["USER_PASSWORD"] != DBNull.Value) ? reader["USER_PASSWORD"].ToString() : "";
                        loginResponse.RoleName = (reader["ROLE_NAME"] != DBNull.Value) ? reader["ROLE_NAME"].ToString() : "";
                        ListloginResponse.Add(loginResponse);
                    }

                    connection.Close();
                }

                return ListloginResponse;
            }
            catch (Exception ex)
            {
                string msg = $"[GetUserByEmail], {ex.Message}";
                throw new(msg);
            }
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            var parts = hashedPassword.Split('.', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var passwordHash = parts[1];

            string hased = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 258 / 8
            ));

            return passwordHash == hased;
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return View("SignIn");
        }
    }
}
