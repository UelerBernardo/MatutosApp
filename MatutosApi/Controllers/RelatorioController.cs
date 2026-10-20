using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("relatorio")]
    public class RelatorioController : ControllerBase
    {
        private readonly MatutosDbContext _dbContext;

        public RelatorioController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("faturamento")]
        [Authorize]
        public async Task<IActionResult> GerarRelatorioFaturamento([FromBody] FiltroFaturamentoDTO filtro)
        {
            try
            {
                var relatorio = await _dbContext.Database.SqlQuery<RelatorioFaturamentoResponseDTO>(
                    $"CALL usp_Relatorio_Faturamento({filtro.DataInicio}, {filtro.DataFim}, {filtro.DescricaoServico}, {filtro.NomeCliente})").ToListAsync();

                return Ok(relatorio);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao gerar relatório de faturamento: {ex.Message}" });
            }
        }

        [HttpPost("servicos-barbeiro")]
        [Authorize]
        public async Task<IActionResult> ConsultarRelatorioServicosPorBarbeiro([FromBody] FiltroServicosPorBarbeiroDTO filtro)
        {
            try
            {
                var relatorio = await _dbContext.Database.SqlQuery<RelatorioServicosPorBarbeiroResponseDTO>(
                    $"CALL usp_Relatorio_ServicosPorBarbeiro({filtro.DataInicio}, {filtro.DataFim}, {filtro.NomeBarbeiro})"
                ).ToListAsync();

                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao gerar relatório de barbeiros: {ex.Message}" });
            }
        }
        [HttpGet("clientes-nao-fidelizados")]
        [Authorize]
        public async Task<IActionResult> ConsultarRelatorioClientesNaoFidelizados([FromQuery] int? qtdDias)
        {
            try
            {
                // O EF Core vai trocar o {filtro.QuantidadeDias} pelo número de forma segura (ou NULL)
                var relatorio = await _dbContext.Database.SqlQuery<RelatorioClientesNaoFidelizadosResponseDTO>(
                    $"CALL usp_Relatorio_ClientesNaoFidelizados({qtdDias})"
                ).ToListAsync();

                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao gerar relatório de clientes não fidelizados: {ex.Message}" });
            }
        }

    }
}
