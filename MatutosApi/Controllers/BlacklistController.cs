using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("blacklist")]
    public class BlacklistController : ControllerBase
    {
        private readonly MatutosDbContext _dbContext;

        public BlacklistController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarBlacklist()
        {
            try
            {
                var hoje = DateTime.Today;

                var blacklists = await _dbContext.Blacklists
                    .Where(b => b.Inicio_Bloqueio >= hoje)
                    .Select(b => new
                    {
                        b.Codigo_BlackList,
                        b.Inicio_Bloqueio,
                        b.Fim_Bloqueio,
                        b.Ativo,
                        b.Detalhes,
                        b.Codigo_Agendamento,

                        UsuariosBloqueados = b.UsuariosBloqueados.Select(ub => ub.Codigo_Usuario).ToList()
                    }).ToListAsync();

                return Ok(blacklists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao buscar blacklist: {ex.Message}" });
            }
        }

        [HttpPost("cadastrar")]
        [Authorize]
        public async Task<IActionResult> CadastrarBlacklist([FromBody] Blacklist blacklist)
        {
            try
            {
                var novaBlacklist = new Blacklist
                {
                    Fim_Bloqueio = blacklist.Fim_Bloqueio,
                    Inicio_Bloqueio = blacklist.Inicio_Bloqueio,
                    Ativo = blacklist.Ativo,
                    Detalhes = blacklist.Detalhes,
                    Codigo_Agendamento = blacklist.Codigo_Agendamento,
                    UsuariosBloqueados = blacklist.UsuariosBloqueados?.Select(ub => new Usuario_Blacklist
                    {
                        Codigo_Usuario = ub.Codigo_Usuario
                    }).ToList() ?? new List<Usuario_Blacklist>()
                };

                _dbContext.Blacklists.Add(novaBlacklist);
                await _dbContext.SaveChangesAsync();
                return Ok(new { Message = "Blacklist cadastrada com sucesso!" });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Message = $"Erro interno: {erroReal}" });
            }
        }
    }
}
