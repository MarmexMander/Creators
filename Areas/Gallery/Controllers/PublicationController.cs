using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Creators.Models;

namespace Creators.Areas.Gallery.Controllers
{
    [Area("Gallery")]
    public class PublicationController : Controller
    {
        public PublicationController()
        {
        }

        [HttpGet("{id:Guid}")]
        public IActionResult Show(Guid id)
        {
            return View();
        }

        [HttpPost("comment")]
        public async Task<ActionResult<Comment>> PostComment(Comment model)
        {
            // TODO: Your code here
            await Task.Yield();
        
            return null;
        }
        
    }
}