using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Creators.Models;
using Creators.Data;
using Microsoft.EntityFrameworkCore;

namespace Creators.Controllers
{
    [Route("api/{action}")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        CreatorsDbContext _dbContext;

        public ApiController(CreatorsDbContext creatorsDbContext)
        {
            _dbContext = creatorsDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> SearchTags(string query, int max = 100)
        {
            var tags = await _dbContext.Tags
            .AsNoTracking()
            .Where(t => t.Name.Contains(query))
            .Take(max)
            .Select(t => t.Name)
            .ToListAsync();

            return tags;
        }
    }
}