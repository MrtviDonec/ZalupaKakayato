using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SectionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SectionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var sections = new List<object>();

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT id_section, section_title, description, section_order 
                        FROM theory_sections 
                        ORDER BY section_order", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sections.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Order = reader.GetInt32(3)
                            });
                        }
                    }
                }

                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
