using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route ("barbeiro")]
    public class BarbeiroController : Controller
    {
        private MatutosDbContext _dbContext;

        public BarbeiroController(MatutosDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet("consultar")]
        public async Task<IActionResult> ConsultarBarbeiro()
        {
            try
            {
                var barbeiros = await _dbContext.Barbeiros
                    .Where(a => a.Ativo == true)
                    .Select(a => new
                    {
                        a.Codigo_Usuario,
                        a.Nome,
                        a.Imagem_Usuario,
                    })
                    .ToListAsync(); 
                return Ok(barbeiros);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = $"Erro interno ao buscar barbeiros: {ex.Message}" });
            }
        }
    }
}
