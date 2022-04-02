using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MembersController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<MembersController> _logger;

        public MembersController(ILogger<MembersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<MemberEntity> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new MemberEntity
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        public MemberEntity GetMember(int id)
        {
            return new MemberEntity()
            {
                Date = DateTime.Now.AddDays(id),
                TemperatureC = 87,
                Summary = "demo"
            };
        }
    }
}
