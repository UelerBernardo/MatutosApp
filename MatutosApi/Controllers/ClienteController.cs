using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route ("cliente")]
    public class ClienteController : Controller
    {
        private readonly MatutosDbContext _context;

        public ClienteController(MatutosDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        //[HttpGet("consultar")]
        //[Authorize]
        //public async Task<IActionResult> ConsultarClientePerfil()
        //{
        //    try
        //    {
        //        var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        //                        ?? User.FindFirst("id")?.Value;

        //        if (string.IsNullOrEmpty(usuario) || !int.TryParse(usuario, out int codigoCliente))
        //        {
        //            return Unauthorized(new { Mensagem = "Usuário não autenticado ou token inválido." });
        //        }

        //        var clienteconsulta = await _context.Clientes
        //            .Where(a => a.Codigo_Usuario == codigoCliente)
        //            .Select(a => new
        //            {
        //                a.Codigo_Usuario,
        //                a.Nome,
        //                a.Email,
        //                a.Ativo
        //            }).FirstOrDefaultAsync();

        //        if (clienteconsulta == null)
        //        {
        //            return NotFound(new { Mensagem = "Perfil de cliente não encontrado." });
        //        }

        //        return Ok(clienteconsulta);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Mensagem = $"Erro ao consultar usuário: {ex.Message}" });
        //    }
        //}

        //[HttpPost("alterar/senha")]
        //[Authorize]
        //public async Task<IActionResult> AlterarSenhaPerfil([FromBody] AlterarSenhaRequest dados)
        //{
        //    try
        //    {
        //        var usuarioClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        //                          ?? User.FindFirst("id")?.Value;

        //        if (string.IsNullOrEmpty(usuarioClaim) || !int.TryParse(usuarioClaim, out int codigoUsuarioLogado))
        //        {
        //            return Unauthorized(new { Mensagem = "Usuário inválido ou não autenticado." });
        //        }
        //        var cliente = await _context.Clientes
        //            .FirstOrDefaultAsync(a => a.Codigo_Usuario == codigoUsuarioLogado);

        //        if (cliente == null)
        //        {
        //            return NotFound(new { Mensagem = "Usuário não encontrado." });
        //        }
        //        bool senhaValida = BCrypt.Net.BCrypt.Verify(dados.SenhaAntiga, cliente.Senha);

        //        if (!senhaValida)
        //        {
        //            return BadRequest(new { Mensagem = "Senha atual está incorreta." });
        //        }
        //        cliente.Senha = BCrypt.Net.BCrypt.HashPassword(dados.SenhaNova);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { Mensagem = "Senha alterada com sucesso!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Mensagem = $"Erro ao alterar senha: {ex.Message}" });
        //    }
        //}

        //public class AlterarSenhaRequest
        //{
        //    public string SenhaAntiga { get; set; }
        //    public string SenhaNova { get; set; }
        //}
    }
}
