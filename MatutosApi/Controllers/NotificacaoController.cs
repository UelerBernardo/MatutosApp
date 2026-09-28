using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;

namespace MatutosApi.Controllers
{
    [ApiController]
    [Route("notificacao")]
    public class NotificacaoController : ControllerBase
    {
        private MatutosDbContext _dbContext;

        public NotificacaoController(MatutosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPut("visualizar-todas")]
        [Authorize]
        public async Task<IActionResult> VisualizarTodas()
        {
            try
            {
                var usuarioLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
                int idPessoaLogada = int.Parse(usuarioLogado);

                int linhasAfetadas = await _dbContext.Notificacoes
                    .Where(not => not.Codigo_Usuario == idPessoaLogada && not.Lida == false)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(not => not.Lida, true)); // Valor true chumbado aqui!

                if (linhasAfetadas == 0)
                {
                    return BadRequest(new { Mensagem = "Todas as notificações já estão lidas." });
                }

                return Ok(new { Mensagem = $"{linhasAfetadas} notificação(ões) marcada(s) como visualizada(s)!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao marcar como visualizada as notificações: {ex.Message}" });
            }
        }

        [HttpGet("notificacao-detalhe/{idHistorico}")]
        [Authorize]
        public async Task<IActionResult> ConsultarDetalhesNotificacao(int idHistorico)
        {
            try
            {
                if (idHistorico <= 0)
                {
                    return BadRequest(new { Mensagem = "Dados inválidos para consultar os detalhes da notificação" });
                }
                var usuarioLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
                int idPessoaLogada = int.Parse(usuarioLogado);

                var not = await _dbContext.Notificacoes.FirstOrDefaultAsync(n => n.Codigo_Historico == idHistorico);
                if (not == null)
                {
                    return NotFound(new { Mensagem = "Notificação não encontrada." });
                }
                if (not.Codigo_Usuario != idPessoaLogada)
                {
                    return Forbid(); // Retorna 403 (Proibido)
                }

                if (!not.Lida)
                {
                    not.Lida = true;
                    await _dbContext.SaveChangesAsync();
                }

                // 5. Faz a consulta detalhada com os Joins que você já tinha feito brilhantemente
                var notDetalhes = await (
                    from notificacao in _dbContext.Notificacoes
                    join cliente in _dbContext.Clientes on
                        notificacao.Codigo_Usuario equals cliente.Codigo_Usuario
                    join regra in _dbContext.Configura_Notificacoes on
                        notificacao.Codigo_Notificacao equals regra.Codigo_Notificacao
                    where (notificacao.Codigo_Historico == idHistorico)
                    select new NotificacaoDetalhes
                    {
                        Codigo_Historico = idHistorico,
                        Codigo_Notificacao = notificacao.Codigo_Notificacao,
                        Codigo_Usuario = notificacao.Codigo_Usuario,
                        NomeCliente = cliente.Nome,
                        MensagemEnviada = notificacao.MensagemEnviada,
                        DataDisparo = notificacao.DataDisparo,
                        Lida = notificacao.Lida, // Como já salvamos acima, aqui já vai voltar "true"
                        Descricao = regra.Descricao,
                    }).FirstOrDefaultAsync();

                return Ok(notDetalhes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar detalhes da notificação: {ex.Message}" });
            }
        }

        [HttpGet ("notificacao-consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarNotificacao([FromQuery] DateTime dataInicial, DateTime dataFinal)
        {
            var usuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            int codigoUsuarioLogado = int.Parse(usuario);

            if (string.IsNullOrEmpty(usuario))
            {
                return BadRequest(new { Mensagem = "Usuário não identificado no token." });
            }

            try
            {
                var listaDeNotificacao = await _dbContext.Notificacoes
                    .Where(n => n.Codigo_Usuario == codigoUsuarioLogado && n.DataDisparo >= dataInicial && n.DataDisparo <= dataFinal)
                    .Join(
                        _dbContext.Configura_Notificacoes,
                        notificacao => notificacao.Codigo_Notificacao,
                        configuracao => configuracao.Codigo_Notificacao,
                        (notificacao, configuracao) => new
                        {
                            Codigo_Historico = notificacao.Codigo_Historico,
                            ConfiguraNotificacao = notificacao.Codigo_Notificacao,
                            Mensagem = notificacao.MensagemEnviada,
                            DataDisparo = notificacao.DataDisparo,
                            Lida = notificacao.Lida,
                            DescricaoRegra = configuracao.Descricao,
                            TipoEvento = configuracao.Codigo_Tipo
                        })
                    .ToListAsync();

                return Ok(listaDeNotificacao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar notificações: {ex.Message}" });
            }

        }

        [HttpGet("regra-consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarRegraNotificacao([FromQuery] DateTime inicial, DateTime final, bool? ativo)
        {
            try
            {
                var listaRegraNotificacao = await _dbContext.Configura_Notificacoes
                    .Where(regra => regra.DataCadastro >= inicial && regra.DataCadastro <= final && regra.Ativo == ativo || !ativo.HasValue)
                    .ToListAsync();

                return Ok(listaRegraNotificacao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar as regras de notificações: {ex.Message}" });
            }
        }

        [HttpGet("tipo-evento/consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarTipoEvento()
        {
            try
            {
                var tipoEvento = await _dbContext.Tipo_Eventos.ToListAsync();

                return Ok(tipoEvento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro ao consultar tipos de eventos: {ex.Message}" });
            }
        }

        [HttpPut ("regra-alterar")]
        [Authorize]
        public async Task<IActionResult> AlterarRegraNotificacao([FromBody] Configura_Notificacao notificacaoAlteracao)
        {
            try
            {
                var regraNotificacaoAlterada = await _dbContext.Configura_Notificacoes
                    .Where(rn => rn.Codigo_Notificacao == notificacaoAlteracao.Codigo_Notificacao)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(n => n.Ativo, notificacaoAlteracao.Ativo)
                        .SetProperty(n => n.Descricao, notificacaoAlteracao.Descricao)
                        .SetProperty(n => n.UnidadeTempo, notificacaoAlteracao.UnidadeTempo)
                        .SetProperty(n => n.Mensagem, notificacaoAlteracao.Mensagem)
                        .SetProperty(n => n.Codigo_Tipo, notificacaoAlteracao.Codigo_Tipo));

                if(regraNotificacaoAlterada <= 0)
                {
                    return NotFound( new {Mensagem = "Regra de notifiacação não encontrada."});
                }

                return Ok(new { Mensagem = "Regra alterada com sucesso!" });
            }
            catch(Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { Mensagem = $"Crash na API: {erroReal}" });
            }
        }


        [HttpPost("regra-cadastrar")]
        [Authorize]
        public async Task<IActionResult> CadastrarNotificacao([FromBody] Configura_Notificacao notificacaoNova)
        {
            try
            {
                // 1. Criamos a entidade que vai ser salva no banco
                var configuracaoNotificacao = new Configura_Notificacao
                {
                    Ativo = notificacaoNova.Ativo,
                    Codigo_Tipo = notificacaoNova.Codigo_Tipo,
                    Descricao = notificacaoNova.Descricao,
                    Mensagem = notificacaoNova.Mensagem,
                    Valor = notificacaoNova.Valor,
                    UnidadeTempo = notificacaoNova.UnidadeTempo,
                    DataCadastro = DateTime.Now
                };

                // 2. Salvamos no banco (Aqui o Entity Framework preenche o ID gerado)
                _dbContext.Configura_Notificacoes.Add(configuracaoNotificacao);
                await _dbContext.SaveChangesAsync();

                // 3. Retornamos os dados atualizados com o ID real do banco
                return Ok(new
                {
                    Mensagem = "Configuração de notificação cadastrada com sucesso.",
                    Codigo_Notificacao = configuracaoNotificacao.Codigo_Notificacao, // 👉 Agora retorna o ID correto!
                    Valor = configuracaoNotificacao.Valor,
                    Ativo = configuracaoNotificacao.Ativo,
                    Codigo_Tipo = configuracaoNotificacao.Codigo_Tipo,
                    TextoMensagem = configuracaoNotificacao.Mensagem,
                    Descricao = configuracaoNotificacao.Descricao,
                    UnidadeTempo = configuracaoNotificacao.UnidadeTempo
                });
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return StatusCode(500, new { Mensagem = $"Erro ao cadastrar regra: {erroReal}" });
            }
        }
    }
}
