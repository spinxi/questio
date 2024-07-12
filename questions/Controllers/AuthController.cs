using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using questions.Helper;
using questions.Models;
using System.Data;

namespace questions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto userDto)
        {
            using (var connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new OracleCommand("pkg_gk_questions.RegisterUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_username", userDto.Username);
                    command.Parameters.Add("p_password", userDto.Password);
                    command.Parameters.Add("p_role", userDto.Role);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto userDto)
        {
            using (var connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new OracleCommand("pkg_gk_questions.AuthenticateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_username", userDto.Username);
                    command.Parameters.Add("p_password", userDto.Password);
                    command.Parameters.Add("p_user_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    command.Parameters.Add("p_role", OracleDbType.Varchar2, 10).Direction = ParameterDirection.Output;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var userId = Convert.ToInt32(command.Parameters["p_user_id"].Value.ToString());
                    var role = command.Parameters["p_role"].Value.ToString();
                    if (userId > 0)
                    {
                        var token = TokenHelper.GenerateToken();

                        using (var tokenCommand = new OracleCommand("BEGIN pkg_gk_questions.SaveUserToken(:userId, :token, :expiry); END;", connection))
                        {
                            tokenCommand.Parameters.Add("userId", userId);
                            tokenCommand.Parameters.Add("token", token);
                            tokenCommand.Parameters.Add("expiry", DateTime.Now.AddHours(1));

                            tokenCommand.ExecuteNonQuery();
                        }

                        return Ok(new { Token = token, Role = role });
                    }
                    return Unauthorized();
                }
            }
        }
    }
}



