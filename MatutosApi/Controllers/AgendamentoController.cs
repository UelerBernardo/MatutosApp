
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

        [HttpPatch("alterarSituacao/{codigoAgendamento}")]
        [Authorize]
        public async Task<IActionResult> AlterarSituacaoAgendamento(int codigoAgendamento, [FromBody] AgendamentoSituacao agendamentoSituacao)
        {
            if(codigoAgendamento <= 0)
            {
                return BadRequest(new { Mensagem = "Código de agendamento inválido." });
            }

            try
            {
                var agendamento = await _dbContext.Agendamentos.FirstOrDefaultAsync(a => a.Codigo_Agendamento == codigoAgendamento);

                if(agendamento == null)
                {
                    return BadRequest(new { Mensagem = "Código de agendamento não encontrado." });
                }
                
                if (agendamentoSituacao == AgendamentoSituacao.Liberado)
                {
                    var existeBloqueio = await _dbContext.Blacklists
                        .Where(b => b.Ativo &&
                                    b.Inicio_Bloqueio < agendamento.Data_Fim_Agendamento &&
                                    b.Fim_Bloqueio > agendamento.Data_Agendamento &&
                                    b.UsuariosBloqueados.Any(ub => ub.Codigo_Usuario == agendamento.Codigo_Barbeiro))
                        .AnyAsync();

                    if(existeBloqueio)
                    {
                        return BadRequest(new { Mensagem = "Não é possível liberar este agendamento, pois existe um bloqueio na agenda para este barbeiro neste horário." });
                    }

                    var bloqueioHorario = new Blacklist
                    {
                        Inicio_Bloqueio = agendamento.Data_Agendamento.Value,
                        Fim_Bloqueio = agendamento.Data_Fim_Agendamento.Value,
                        Ativo = true,
                        Detalhes = $"Horário bloqueado oriundo do agendamento {codigoAgendamento}.",
                        Codigo_Agendamento = codigoAgendamento,

                        UsuariosBloqueados = new List<Usuario_Blacklist>
                        {
                            new Usuario_Blacklist
                            {
                                Codigo_Usuario = agendamento.Codigo_Barbeiro
                            }
                        }
                    };

                    _dbContext.Blacklists.Add(bloqueioHorario);
                }

                if(agendamentoSituacao == AgendamentoSituacao.Cancelado)
                {
                    var inativarBlacklist = await _dbContext.Blacklists
                        .Where(b => b.Codigo_Agendamento == codigoAgendamento)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(bc => bc.Ativo, false)
                        );
                }

                agendamento.Codigo_Situacao_Agendamento = agendamentoSituacao;
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Mensagem = "Situação alterada com sucesso!",
                    CodigoSituacaoNova = agendamentoSituacao
                    
                });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao cancelar o agendamento: {ex.Message}" });
            }
        }

        [HttpPatch("inativarAgendamento/{codigoAgendamento}")]
        [Authorize]
        public async Task<IActionResult> ExcluirAgendamento(int codigoAgendamento)
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
                agendamento.Codigo_Situacao_Agendamento = AgendamentoSituacao.Cancelado;

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

                // 1. Descobre de qual agendamento estamos falando
                int codigoAgendamento = listaServicos.First().Codigo_Agendamento;

                // 2. Busca a "Capa" do agendamento ANTES de tudo, para ter a Data de Início e o Barbeiro
                var agendamentoCapa = await _dbContext.Agendamentos.FirstOrDefaultAsync(a => a.Codigo_Agendamento == codigoAgendamento);

                if (agendamentoCapa == null || !agendamentoCapa.Data_Agendamento.HasValue)
                {
                    return BadRequest(new { Mensagem = "Agendamento principal não encontrado ou sem data de início definida." });
                }

                // 3. Calcula o Tempo Total e projeta a Data Final (A Estratégia de Projeção!)
                int tempoTotalServico = listaServicos.Sum(x => x.Tempo_Servico_Item);
                DateTime dataFimProjetada = agendamentoCapa.Data_Agendamento.Value.AddMinutes(tempoTotalServico);

                // 4. AGORA SIM: Valida a Blacklist com os dados exatos de Início e Fim contra o barbeiro específico
                var existeBloqueio = await _dbContext.Blacklists
                    .Where(b => b.Ativo &&
                                b.Inicio_Bloqueio < dataFimProjetada &&
                                b.Fim_Bloqueio > agendamentoCapa.Data_Agendamento.Value &&
                                b.UsuariosBloqueados.Any(ub => ub.Codigo_Usuario == agendamentoCapa.Codigo_Barbeiro))
                    .AnyAsync();

                if (existeBloqueio)
                {
                    return BadRequest(new { Mensagem = "O tempo total destes serviços invade um horário bloqueado na agenda do barbeiro. Escolha um horário mais cedo." });
                }

                // 5. Se passou da Blacklist, preparamos os serviços para salvar
                var novosAgendamentosServicos = new List<Agendamento_Servico>();
                decimal somatotal = 0;

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

                    somatotal += item.Valor_Total_Item;
                }
                agendamentoCapa.Valor_Total_Agendamento = somatotal;
                agendamentoCapa.Data_Fim_Agendamento = dataFimProjetada;

                _dbContext.Agendamento_Servicos.AddRange(novosAgendamentosServicos);
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

            var bloqueioAgenda = await _dbContext.Blacklists
                .Where(b => b.Ativo &&
                            b.Inicio_Bloqueio <= agendamento.Data_Agendamento &&
                            b.Fim_Bloqueio > agendamento.Data_Agendamento &&
                            b.UsuariosBloqueados.Any(ub => ub.Codigo_Usuario == agendamento.Codigo_Barbeiro))
                .AnyAsync();
            if(bloqueioAgenda)
            {
                return BadRequest(new { Mensagem = "O horário de início escolhido está indisponível para este barbeiro." });
            }


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

        [HttpGet("consultar/detalhes/{id:int}")]
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
                        a.Valor_Total_Agendamento,
                        a.Codigo_Situacao_Agendamento,

                        Cliente = new { Nome = a.Cliente.Nome },

                        Barbeiro = new { Nome = a.Barbeiro.Nome },
                        Agendamento_Servicos = a.Agendamento_Servicos.Select(s => new
                        {
                            s.Quantidade_Servico,
                            s.Valor_Total_Item,
                            Servico = new { Descricao = s.Servico.Descricao } 
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

        [HttpGet("consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarAgendamento()
        {
            var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

            int codigoUsuarioLogado = int.Parse(usuario);
            try
            {
                var agendamento = await _dbContext.Agendamentos
                    .Where(a => a.Codigo_Cliente == codigoUsuarioLogado && a.Ativo == true && a.Data_Agendamento >= DateTime.Today.AddMonths(-1))
                    .Select(a => new
                    {
                        a.Codigo_Agendamento,
                        a.Data_Agendamento,
                        a.Valor_Total_Agendamento,
                        a.Codigo_Situacao_Agendamento,

                        Cliente = new { Nome = a.Cliente.Nome},
                        Barbeiro = new {Nome = a.Barbeiro.Nome}
                    })
                    .OrderByDescending(a => a.Codigo_Agendamento)
                    .ToListAsync();

                if (!agendamento.Any())
                {
                    return NotFound(new { Mensagem = $"Você não possui Angedamentos." });
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
