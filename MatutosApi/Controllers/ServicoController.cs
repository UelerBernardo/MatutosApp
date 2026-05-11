using MatutosApi.Infraestrutura;
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

        [HttpGet ("consultar")]
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
