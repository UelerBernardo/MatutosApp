
using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionVisitors.Internal;

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
        [HttpGet("consultar/horarios-livres")]
        [Authorize]
        public async Task<IActionResult> AgendamentoHorariosLivres([FromQuery] int codigoBarbeiro, [FromQuery] DateTime dataAgendamento)
        {
            try
            {
                if (codigoBarbeiro <= 0 || dataAgendamento == DateTime.MinValue)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos para a consulta de horários disponíveis" });
                }

                // 1. Regra de Negócio: Domingos a barbearia está fechada
                if (dataAgendamento.DayOfWeek == DayOfWeek.Sunday)
                {
                    return Ok(new List<string>()); // Retorna lista vazia, nenhum horário disponível
                }

                // 2. Parâmetros de funcionamento (Pode adaptar o intervalo de acordo com o serviço)
                TimeSpan horarioAbertura = new TimeSpan(7, 30, 0);  // 07:30
                TimeSpan horarioFechamento = new TimeSpan(18, 30, 0); // 18:30
                int intervaloMinutos = 30; // Tempo estimado de cada corte (ex: 30 minutos)

                // 3. Buscar Bloqueios (Blacklist) que passam por esse dia
                var bloqueios = await (
                    from blacklist in _dbContext.Blacklists
                    join usuariobloqueio in _dbContext.Usuario_Blacklists on blacklist.Codigo_BlackList equals usuariobloqueio.Codigo_BlackList
                    where usuariobloqueio.Codigo_Usuario == codigoBarbeiro
                       && blacklist.Ativo == true
                       && blacklist.Inicio_Bloqueio.Date <= dataAgendamento.Date
                       && blacklist.Fim_Bloqueio.Date >= dataAgendamento.Date
                    select new
                    {
                        Inicio = blacklist.Inicio_Bloqueio.TimeOfDay,
                        Fim = blacklist.Fim_Bloqueio.TimeOfDay
                    }).ToListAsync();

                // 4. Buscar Agendamentos de clientes que já estão marcados para este dia
                // Precisamos garantir que o barbeiro não receba dois clientes na mesma hora
                var agendamentosMarcados = await _dbContext.Agendamentos
                    .Where(a => a.Codigo_Barbeiro == codigoBarbeiro
             // Adicionado o .Value antes do .Date
                     && a.Data_Agendamento.Value.Date == dataAgendamento.Date
                     && a.Codigo_Situacao_Agendamento != AgendamentoSituacao.Cancelado)
                    .Select(a => new
                    {
                        // Adicionado o .Value antes do .TimeOfDay
                        Inicio = a.Data_Agendamento.Value.TimeOfDay,
                        Fim = a.Data_Fim_Agendamento.Value.TimeOfDay
                    }).ToListAsync();

                // 5. Montar a grade de horários livres
                var horariosLivres = new List<string>();
                var horaAtual = horarioAbertura;

                // O laço roda enquanto o horário atual + o tempo do corte couberem dentro do expediente
                while (horaAtual.Add(TimeSpan.FromMinutes(intervaloMinutos)) <= horarioFechamento)
                {
                    var fimDoSlot = horaAtual.Add(TimeSpan.FromMinutes(intervaloMinutos));

                    // A mágica acontece aqui: Verifica se o nosso "Slot" de 30 minutos esbarra em algum bloqueio
                    bool interceptaBlacklist = bloqueios.Any(b => horaAtual < b.Fim && fimDoSlot > b.Inicio);

                    // Verifica se o nosso "Slot" esbarra em algum cliente já agendado
                    bool interceptaAgendamento = agendamentosMarcados.Any(a => horaAtual < a.Fim && fimDoSlot > a.Inicio);

                    // Se não bateu com a Blacklist E não bateu com Agendamento, o horário está Livre!
                    if (!interceptaBlacklist && !interceptaAgendamento)
                    {
                        horariosLivres.Add(horaAtual.ToString(@"hh\:mm"));
                    }

                    // Pula para o próximo horário (Ex: se era 07:30, agora vai checar 08:00)
                    horaAtual = fimDoSlot;
                }

                return Ok(horariosLivres);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao buscar horários livres: {ex.Message}" });
            }
        }

        [HttpGet("meus-servicos")]
        [Authorize]
        public async Task<IActionResult> ConsultarMeusServicos([FromQuery] string? nomeUsuario, [FromQuery] AgendamentoSituacao? situacao, [FromQuery] DateTime dataInicial, [FromQuery] DateTime dataFinal, bool isAdmin)
        {
            try
            {
                var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuario) || !int.TryParse(usuario, out int idPessoaLogada))
                {
                    return BadRequest(new { Mensagem = "Usuário logado não encontrado, faça login e tente novamente" });
                }


                if(isAdmin == false)
                {
                    var meusServicos = await (
                  from agendamento in _dbContext.Agendamentos
                  join cliente in _dbContext.Clientes on
                  agendamento.Codigo_Cliente equals cliente.Codigo_Usuario
                  where agendamento.Codigo_Barbeiro == idPessoaLogada
                     && agendamento.Data_Agendamento >= dataInicial
                     && agendamento.Data_Agendamento <= dataFinal
                     && (string.IsNullOrEmpty(nomeUsuario) || cliente.Nome.Contains(nomeUsuario))
                     && (situacao == null || agendamento.Codigo_Situacao_Agendamento == situacao)

                  select new MeusServicos
                  {
                      Codigo_Agendamento = agendamento.Codigo_Agendamento,
                      Data_Agendamento = agendamento.Data_Agendamento,
                      Codigo_Cliente = agendamento.Codigo_Cliente,
                      Codigo_Situacao_Agendamento = agendamento.Codigo_Situacao_Agendamento,
                      Valor_Total_Agendamento = agendamento.Valor_Total_Agendamento,
                      Ativo = agendamento.Ativo,
                      Data_Fim_Agendamento = agendamento.Data_Fim_Agendamento,
                      Codigo_Barbeiro = agendamento.Codigo_Barbeiro,
                      Nome_Cliente = cliente.Nome
                  }).ToListAsync();
                    return Ok(meusServicos);

                }
                else
                {
                    // Correção no bloco do Admin: Fazer join com Cliente também!
                    var servicosGerais = await (
                        from agendamento in _dbContext.Agendamentos
                        join barbeiro in _dbContext.Barbeiros on agendamento.Codigo_Barbeiro equals barbeiro.Codigo_Usuario
                        join cliente in _dbContext.Clientes on agendamento.Codigo_Cliente equals cliente.Codigo_Usuario // Adicionado!
                        where agendamento.Data_Agendamento >= dataInicial
                           && agendamento.Data_Agendamento <= dataFinal
                           && (string.IsNullOrEmpty(nomeUsuario) || barbeiro.Nome.Contains(nomeUsuario) || cliente.Nome.Contains(nomeUsuario))
                           && (situacao == null || agendamento.Codigo_Situacao_Agendamento == situacao)

                        select new MeusServicos
                        {
                            Codigo_Agendamento = agendamento.Codigo_Agendamento,
                            Data_Agendamento = agendamento.Data_Agendamento,
                            Codigo_Cliente = agendamento.Codigo_Cliente,
                            Codigo_Situacao_Agendamento = agendamento.Codigo_Situacao_Agendamento,
                            Valor_Total_Agendamento = agendamento.Valor_Total_Agendamento,
                            Ativo = agendamento.Ativo,
                            Data_Fim_Agendamento = agendamento.Data_Fim_Agendamento,
                            Codigo_Barbeiro = agendamento.Codigo_Barbeiro,
                            Nome_Cliente = cliente.Nome // Corrigido!
                        }).ToListAsync();

                    return Ok(servicosGerais);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro interno ao consultar os serviços: {ex.Message}" });
            }
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

                if (agendamentoSituacao == AgendamentoSituacao.Cancelado)
                {
                    // Busca a lista na base de dados para a memória do EF Core
                    var blacklistsParaInativar = await _dbContext.Blacklists
                        .Where(b => b.Codigo_Agendamento == codigoAgendamento)
                        .ToListAsync();

                    // Altera o estado de cada registo individualmente
                    foreach (var blacklist in blacklistsParaInativar)
                    {
                        blacklist.Ativo = false;
                    }
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

        [HttpGet("meus-agendamentos")]
        [Authorize]
        public async Task<IActionResult> ConsultarAgendamento([FromQuery] string? nomeBarbeiro, AgendamentoSituacao? situacao, DateTime datainicial, DateTime datafinal)
        {
            var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

            int idUsuario = int.Parse(usuario);

            var usuarioLogado = await _dbContext.Usuarios.Where(u => u.Codigo_Usuario == idUsuario).FirstOrDefaultAsync();

            try
            {
                var meusAgendamentos = await (
                    from agendamento in _dbContext.Agendamentos
                    join barbeiro in _dbContext.Barbeiros on
                        agendamento.Codigo_Barbeiro equals barbeiro.Codigo_Usuario
                    where (agendamento.Ativo == true) &&
                            (agendamento.Codigo_Cliente == idUsuario) &&
                            (agendamento.Data_Agendamento >= datainicial) &&
                            (agendamento.Data_Agendamento <= datafinal) &&
                            (string.IsNullOrWhiteSpace(nomeBarbeiro) || barbeiro.Nome.Contains(nomeBarbeiro)) &&
                            (situacao == null || agendamento.Codigo_Situacao_Agendamento == situacao)
                    select new MeusAgendamentos
                    {
                        Codigo_Agendamento = agendamento.Codigo_Agendamento,
                        Data_Agendamento = agendamento.Data_Agendamento,
                        Data_Fim_Agendamento = agendamento.Data_Fim_Agendamento,
                        Ativo = agendamento.Ativo,
                        Valor_Total_Agendamento = agendamento.Valor_Total_Agendamento,
                        Codigo_Cliente = agendamento.Codigo_Cliente,
                        Codigo_Barbeiro = agendamento.Codigo_Barbeiro,
                        Codigo_Situacao_Agendamento = agendamento.Codigo_Situacao_Agendamento,
                        NomeBarbeiro = barbeiro.Nome,
                        NomeCliente = usuarioLogado.Nome
                    }).ToListAsync();

                return Ok(meusAgendamentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar agendamento: {ex.Message}" });
            }
        }
    }
}
