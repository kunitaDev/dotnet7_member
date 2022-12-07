using Microsoft.Extensions.Configuration;

namespace dotnet7_member.Services
{
    public class SqlConnectionContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _conn;

        public SqlConnectionContext(IConfiguration configuration)
        {
            this._configuration = configuration;
            _conn = _configuration["ConnectionStrings:DefaultConnection"];
        }

        public string GetConnectionString() => _conn;
    }
}
