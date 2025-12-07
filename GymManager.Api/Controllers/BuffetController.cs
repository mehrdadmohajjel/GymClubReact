using GymManager.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuffetController : ControllerBase
    {
        private static List<Buffet> _buffets = new List<Buffet>();
        private static int _id = 1;

        [HttpGet]
        public IActionResult GetAll() => Ok(_buffets);

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var item = _buffets.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Create(Buffet b)
        {
            b.Id = _id++;
            _buffets.Add(b);
            return Ok(b);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Buffet b)
        {
            var item = _buffets.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            item.Title = b.Title;
            item.Description = b.Description;
            item.Price = b.Price;
            item.IsActive = b.IsActive;

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _buffets.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            _buffets.Remove(item);
            return Ok();
        }
    }
}
