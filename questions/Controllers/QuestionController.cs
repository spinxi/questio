using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using questions.Models;
using System.Data;
using System.Reflection;

namespace questions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public QuestionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult AddQuestion([FromBody] QuestionDto questionDto)
        {
            using (var connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_gk_questions.AddQuestion", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_question_text", OracleDbType.Varchar2).Value = questionDto.QuestionText;
                    command.Parameters.Add("p_question_answer", OracleDbType.Varchar2).Value = questionDto.QuestionAnswer;
                    command.Parameters.Add("p_is_mandatory", OracleDbType.Int32).Value = questionDto.IsMandatory;

                    command.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult EditQuestion(int id, [FromBody] QuestionDto questionDto)
        {
            using (var connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new OracleCommand("pkg_gk_questions.EditQuestion", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_id", id);
                    command.Parameters.Add("p_question_text", questionDto.QuestionText);
                    command.Parameters.Add("p_is_mandatory", questionDto.IsMandatory ? 1 : 0);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return Ok();
        }


        [HttpGet]
        public IActionResult GetQuestions()
        {
            using (var connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new OracleCommand("pkg_gk_questions.GetQuestions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        var questions = new List<QuestionDto>();
                        while (reader.Read())
                        {
                            questions.Add(new QuestionDto
                            {
                                Id = Convert.ToInt32(reader["ID"]),
                                QuestionText = reader["QuestionText"].ToString(),
                                QuestionAnswer = reader["QuestionAnswer"].ToString(),
                                IsMandatory = Convert.ToBoolean(reader["IsMandatory"])
                            });
                        }
                        return Ok(questions);
                    }
                }
            }
        }


        [HttpGet("{userId}")]
        public IActionResult GetUserResponses(int userId)
        {
            try
            {
                List<UserResponseDto> responses = new List<UserResponseDto>();

                using (OracleConnection connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (OracleCommand command = new OracleCommand("pkg_gk_questions.GetUserResponses", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        command.Parameters.Add("p_user_responses", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        connection.Open();

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var response = new UserResponseDto
                                {
                                    Username = reader["username"].ToString(),
                                    QuestionText = reader["QuestionText"].ToString(),
                                    ResponseText = reader["responseText"].ToString()
                                };
                                responses.Add(response);
                            }
                        }
                    }
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    



    [HttpGet("responses")]
        public IActionResult GetUserResponses()
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (OracleCommand command = new OracleCommand("pkg_gk_questions.GetUserResponses", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("p_user_responses", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        connection.Open();

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            List<object> responses = new List<object>();
                            while (reader.Read())
                            {
                                var response = new
                                {
                                    Username = reader["username"].ToString(),
                                    QuestionText = reader["question_text"].ToString(),
                                    ResponseText = reader["response_text"].ToString()
                                };
                                responses.Add(response);
                            }
                            return Ok(responses);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("submit")]
        public IActionResult SubmitResponses([FromBody] List<ResponseDto> responseDtos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                using (OracleConnection connection = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    foreach (var responseDto in responseDtos)
                    {
                        using (OracleCommand command = new OracleCommand("pkg_gk_questions.SubmitResponse", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = responseDto.UserId;
                            command.Parameters.Add("p_question_id", OracleDbType.Int32).Value = responseDto.QuestionId;
                            command.Parameters.Add("p_response_text", OracleDbType.Varchar2).Value = responseDto.ResponseText;

                            command.ExecuteNonQuery();
                        }
                    }

                    return Ok(new { Message = "Responses submitted successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



    }

}
