using MemoLingo.Services;
using MemoLingo.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    /// <summary>
    /// Recebe e devolve models, delegando toda a lógica de negócio para o UserService.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserModel>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserModel>> Create(UserModel user)
        {
            try
            {
                var created = await _userService.CreateAsync(user);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UserModel user)
        {
            var updated = await _userService.UpdateAsync(id, user);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remove(int id)
        {
            var removed = await _userService.RemoveAsync(id);
            return removed ? NoContent() : NotFound();
        }
    }
}
