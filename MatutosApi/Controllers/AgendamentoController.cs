using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("agendamento")]
    public class AgendamentoController : ControllerBase
    {
        private MatutosDbContext _dbContext;

        public AgendamentoController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost("cadastrar")]
        [Authorize]
        public async Task<IActionResult> CriarAgendamento([FromBody] Agendamento agendamento )
        {

            var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(usuario))
            {
                return BadRequest(new { Mensagem = "Usuário não identificado no token." });
            }

            int codigoUsuarioLogado  = int.Parse(usuario);

            var agendamentoNovo = new Agendamento
            {
                Data_Agendamento = agendamento.Data_Agendamento,
                Data_Fim_Agendamento = null, //Validar se pode ser null de inicio, e depois de selecionar o serviço faremos o cálculo
                Codigo_Barbeiro = agendamento.Codigo_Barbeiro,
                Codigo_Cliente = codigoUsuarioLogado,
                Codigo_Situacao_Agendamento = AgendamentoSituacao.Aberto
            };

            _dbContext.Agendamentos.Add(agendamentoNovo);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Mensagem = "Agendamento realizado com sucesso!",
                agendamentoNovo.Codigo_Agendamento,
                agendamentoNovo.Data_Agendamento,
                agendamentoNovo.Data_Fim_Agendamento,
                agendamentoNovo.Codigo_Cliente,
                agendamentoNovo.Codigo_Barbeiro
            });
        }
    }
}
