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
                if(blacklistAlteracao == null)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos para alterar o bloqueio de agenda!" });
                }

                var alteracao = await _dbContext.Blacklists
                    .Where(blacklist => blacklist.Codigo_BlackList == blacklistAlteracao.Codigo_BlackList)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(b => b.Ativo, blacklistAlteracao.Ativo)
                        .SetProperty(b => b.Inicio_Bloqueio, blacklistAlteracao.Inicio_Bloqueio)
                        .SetProperty(b => b.Fim_Bloqueio, blacklistAlteracao.Fim_Bloqueio)
                        .SetProperty(b => b.Detalhes, blacklistAlteracao.Detalhes));


                if(alteracao <= 0)
                {
                    return BadRequest(new { Mensagem = "Nenhum bloqueio encotrado para alteração" });
                }

                return Ok(new { Mensagem = "Bloqueio de agenda alterado com sucesso." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao buscar blacklist: {ex.Message}" });
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
