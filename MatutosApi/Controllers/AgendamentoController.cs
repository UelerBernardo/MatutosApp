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

        [HttpPatch("inativarAgendamento/{codigoAgendamento}")]
        [Authorize]
        public async Task<IActionResult> InativarAgendamento(int codigoAgendamento)
        {
            if (codigoAgendamento <= 0)
            {
                return BadRequest(new { Mensagem = "Código de agendamento inválido." });
            }

            try
            {
                var agendamento = await _dbContext.Agendamentos
                                                  .FirstOrDefaultAsync(a => a.Codigo_Agendamento == codigoAgendamento);

                if (agendamento == null)
                {
                    return NotFound(new { Mensagem = "Agendamento não encontrado no banco de dados." });
                }

                agendamento.Ativo = false;

                await _dbContext.SaveChangesAsync();

                return Ok(new { Mensagem = "Agendamento cancelado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao cancelar o agendamento: {ex.Message}" });
            }
        }

        [HttpPost("cadastrar/agendamentoServico")]
        [Authorize]
        public async Task<IActionResult> CriarAgendamentoServico([FromBody] List<Agendamento_Servico> listaServicos)
        {
            try
            {
                if (listaServicos == null || !listaServicos.Any())
                {
                    return BadRequest(new { Mensagem = "Nenhum serviço foi selecionado para este agendamento." });
                }

                // 3. Montamos uma nova lista limpa apenas com os dados que importam
                var novosAgendamentosServicos = new List<Agendamento_Servico>();

                foreach (var item in listaServicos)
                {
                    novosAgendamentosServicos.Add(new Agendamento_Servico
                    {
                        Codigo_Agendamento = item.Codigo_Agendamento,
                        Codigo_Servico = item.Codigo_Servico,
                        Quantidade_Servico = item.Quantidade_Servico > 0 ? item.Quantidade_Servico : 1,
                        Valor_Total_Item = item.Valor_Total_Item,
                        Tempo_Servico_Item = item.Tempo_Servico_Item,
                    });
                }

                // 4. Salva a lista inteira de uma vez no banco de dados!
                _dbContext.Agendamento_Servicos.AddRange(novosAgendamentosServicos);

                int codigoAgendamento = novosAgendamentosServicos.First().Codigo_Agendamento;

                var agendamentoCapa = await _dbContext.Agendamentos.FirstOrDefaultAsync(a => a.Codigo_Agendamento == codigoAgendamento);

                if (agendamentoCapa != null)
                {
                   decimal somatotal = novosAgendamentosServicos.Sum(x => x.Valor_Total_Item);

                    agendamentoCapa.Valor_Total_Agendamento = somatotal;

                    int tempoTotalServico = novosAgendamentosServicos.Sum(x => x.Tempo_Servico_Item);

                    if (agendamentoCapa.Data_Agendamento.HasValue)
                    {
                        DateTime dataFim = agendamentoCapa.Data_Agendamento.Value.AddMinutes(tempoTotalServico);
                        agendamentoCapa.Data_Fim_Agendamento = dataFim;
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Codigo_Agendamento = codigoAgendamento,
                    Mensagem = "Serviços vinculados ao agendamento com sucesso!",
                    TotalServicosAdicionados = novosAgendamentosServicos.Count
                });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Mensagem = $"Crash na API: {erroReal}" });
            }
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
                Data_Fim_Agendamento = null, 
                Codigo_Barbeiro = agendamento.Codigo_Barbeiro,
                Codigo_Cliente = codigoUsuarioLogado,
                Codigo_Situacao_Agendamento = AgendamentoSituacao.Aberto,
                Ativo = agendamento.Ativo
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

        [HttpGet("consultar/{id:int}")]
        [Authorize]
        public async Task<IActionResult> ConsultarDetalhesAgendamento(int id)
        {
            try
            {
                var agendamento = await _dbContext.Agendamentos
                    .Where(a => a.Codigo_Agendamento == id)
                    .Select(a => new
                    {
                        a.Codigo_Agendamento,
                        a.Data_Agendamento,
                        a.Data_Fim_Agendamento,

                        // Cliente
                        a.Codigo_Cliente,
                        Nome_Cliente = a.Cliente.Nome,

                        // Barbeiro
                        a.Codigo_Barbeiro,
                        Nome_Barbeiro = a.Barbeiro.Nome,

                        a.Valor_Total_Agendamento,

                        Servicos = a.Agendamento_Servicos.Select(s => new
                        {
                            s.Codigo_Servico,
                            Nome_Servico = s.Servico.Descricao, 
                            s.Quantidade_Servico,
                            s.Valor_Total_Item,
                            s.Tempo_Servico_Item
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (agendamento == null)
                {
                    // Padronizado para retornar JSON
                    return NotFound(new { Mensagem = $"Agendamento com ID {id} não encontrado." });
                }

                return Ok(agendamento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar agendamento: {ex.Message}" });
            }
        }
    }
}
