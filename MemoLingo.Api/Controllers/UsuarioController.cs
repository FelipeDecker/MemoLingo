using MemoLingo.Api.Entities;
using MemoLingo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemoLingo.Api.Controllers
{
    /// <summary>
    /// Recebe e devolve models, delegando toda a lógica de negócio para o UsuarioService.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> ObterTodos()
        {
            var usuarios = await _usuarioService.ObterTodosAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Usuario>> ObterPorId(int id)
        {
            var usuario = await _usuarioService.ObterPorIdAsync(id);
            return usuario is null ? NotFound() : Ok(usuario);
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> Criar(Usuario usuario)
        {
            try
            {
                var criado = await _usuarioService.CriarAsync(usuario);
                return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, Usuario usuario)
        {
            var atualizado = await _usuarioService.AtualizarAsync(id, usuario);
            return atualizado ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remover(int id)
        {
            var removido = await _usuarioService.RemoverAsync(id);
            return removido ? NoContent() : NotFound();
        }
    }
}
