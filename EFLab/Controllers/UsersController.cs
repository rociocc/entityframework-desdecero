using System.Threading.Tasks;
using EFLab.Domain.Entities;
using EFLab.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace EFLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private EFLabContext _context;

        public UsersController(EFLabContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostTodoItem(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(long id)
        {
            var todoItem = await _context.Users.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }
    }
}