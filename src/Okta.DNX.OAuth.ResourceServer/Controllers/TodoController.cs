using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Okta.DNX.OAuth.ResourceServer.Controllers
{
    
    [Route("api/todo")]
    public class TodoController : Controller
    {
        private readonly ILogger _logger;

        public TodoController(Models.ITodoRepository todoItems, ILogger<TodoController> logger)
        {
            TodoItems = todoItems;
            _logger = logger;
        }

        public Models.ITodoRepository TodoItems { get; set; }

        [Authorize("todo.read")]
        [HttpGet]
        public IEnumerable<Models.TodoItem> GetAll()
        {
            _logger.LogInformation("Getting all items of Todo Repository");
            return TodoItems.GetAll();
        }

        [Authorize("todo.read")]
        [HttpGet("{id}", Name = "GetTodo")]
        public IActionResult GetById(string id)
        {
            var item = TodoItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [Authorize("todo.write")]
        [HttpPost]
        public IActionResult Create([FromBody] Models.TodoItem item)
        {
            if (item == null)
            {
                return BadRequest();
            }
            TodoItems.Add(item);
            return CreatedAtRoute("GetTodo", new { id = item.Key }, item);
        }

        [Authorize("todo.write")]
        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] Models.TodoItem item)
        {
            if (item == null || item.Key != id)
            {
                return BadRequest();
            }

            var todo = TodoItems.Find(id);
            if (todo == null)
            {
                return NotFound();
            }

            TodoItems.Update(item);
            return new NoContentResult();
        }

        [Authorize("todo.delete")]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var todo = TodoItems.Find(id);
            if (todo == null)
            {
                return NotFound();
            }

            TodoItems.Remove(id);
            return new NoContentResult();
        }
    }
}
