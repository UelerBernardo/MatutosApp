using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("servico")]
    public class ServicoController : ControllerBase
    {
        private readonly MatutosDbContext _dbContext;

        public ServicoController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost ("cadastrar")]
        [Authorize]
        public async  Task<IActionResult> CadastrarServico([FromBody] Servico servico)
        {

            try
            {
                var servicoNovo = new Servico
                {
                    Descricao = servico.Descricao,
                    Duracao = servico.Duracao,
                    Preco = servico.Preco,
                    Tempo_Servico = servico.Tempo_Servico,
                    Ativo = servico.Ativo
                };

                _dbContext.Servicos.Add(servicoNovo);

                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            catch(Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao buscar barbeiros: {ex.Message}" });
            }
        }


        [HttpGet ("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarServico()
        {
            try
            {
                var servicos = await _dbContext.Servicos
                    .Where(a => a.Ativo == true)
                    .Select(a => new
                    {
                        a.Codigo_Servico,
                        a.Descricao,
                        a.Preco,
                        a.Duracao,
                        a.Tempo_Servico,
                        a.Ativo
                    })
                    .ToListAsync();
                return Ok(servicos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro interno ao buscar barbeiros: {ex.Message}" });
            }
        }
    }
}
