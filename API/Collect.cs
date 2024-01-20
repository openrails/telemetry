using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Open_Rails_Telemetry.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Collect : ControllerBase
    {
        readonly IConfiguration Configuration;
        readonly string DataPathCollectSystem;

        public Collect(IConfiguration configuration)
        {
            Configuration = configuration;
            DataPathCollectSystem = Path.Combine(Configuration["DataPath"], "collect", "system");
            if (!Directory.Exists(DataPathCollectSystem)) Directory.CreateDirectory(DataPathCollectSystem);
        }

        [HttpPost("System")]
        [Consumes("application/json")]
        public async Task<IActionResult> PostSystem([FromBody] JsonElement data)
        {
            // File's name has the date for analysis, and a random key to prevent collisions
            var date = DateTime.UtcNow.Date;
            var randomKey = Guid.NewGuid().ToString();
            var file = new FileInfo(Path.Combine(DataPathCollectSystem, $"{date:yyyy-MM-dd}_{randomKey}.json"));
            // Write the JSON into the file
            await using (var stream = file.Create())
            {
                await JsonSerializer.SerializeAsync(stream, data);
            }
            // Wipe out the file's timestamps so we cannot correlate them with e.g. access logs
            file.CreationTimeUtc = date;
            file.LastWriteTimeUtc = date;
            file.LastAccessTimeUtc = date;
            return Ok();
        }
    }
}
