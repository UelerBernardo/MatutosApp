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

        [HttpPut("alterar")]
        [Authorize]
        public async Task<IActionResult> AlterarBlacklist([FromBody] Blacklist blacklistAlteracao)
        {
            try
            {
                if (blacklistAlteracao == null || blacklistAlteracao.Codigo_BlackList <= 0)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos para alterar o bloqueio de agenda!" });
                }

                // 1. Traz o dado para a memória (ChangeTracker começa a rastrear)
                var blacklistBanco = await _dbContext.Blacklists
                    .FirstOrDefaultAsync(b => b.Codigo_BlackList == blacklistAlteracao.Codigo_BlackList);

                if (blacklistBanco == null)
                {
                    return BadRequest(new { Mensagem = "Nenhum bloqueio encontrado para alteração" });
                }

                // 2. Altera os campos (O EF Core anota silenciosamente o "Antes" e o "Depois")
                blacklistBanco.Ativo = blacklistAlteracao.Ativo;
                blacklistBanco.Inicio_Bloqueio = blacklistAlteracao.Inicio_Bloqueio;
                blacklistBanco.Fim_Bloqueio = blacklistAlteracao.Fim_Bloqueio;
                blacklistBanco.Detalhes = blacklistAlteracao.Detalhes;

                // 3. Efetiva as mudanças e dispara o gatilho da nossa AuditoriaLog
                await _dbContext.SaveChangesAsync();

                return Ok(new { Mensagem = "Bloqueio de agenda alterado com sucesso." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao alterar blacklist: {ex.Message}" });
            }
        }


        [HttpGet("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarBlacklist([FromQuery] DateTime inicial, DateTime final)
        {
            try
            {
                var hoje = DateTime.Today;

                var blacklists = await _dbContext.Blacklists
                    .Where(b => b.Inicio_Bloqueio >= inicial && b.Inicio_Bloqueio <= final)
                    .Select(b => new
                    {
                        b.Codigo_BlackList,
                        b.Inicio_Bloqueio,
                        b.Fim_Bloqueio,
                        b.Ativo,
                        b.Detalhes
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
