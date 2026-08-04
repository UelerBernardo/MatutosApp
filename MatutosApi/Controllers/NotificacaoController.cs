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

        [HttpGet ("notificacao-consultar")]
        [Authorize]
        public async Task<IActionResult> ConsultarNotificacao()
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
                    .Where(n => n.Codigo_Usuario == codigoUsuarioLogado)
                    .Join(
                        _dbContext.Configura_Notificacoes,
                        notificacao => notificacao.Codigo_Notificacao,
                        configuracao => configuracao.Codigo_Notificacao,
                        (notificacao, configuracao) => new
                        {
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
        public async Task<IActionResult> ConsultarRegraNotificacao()
        {
            try
            {
                var listaRegraNotificacao = await _dbContext.Configura_Notificacoes.ToListAsync();

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

                return Ok("Regra alterada com sucesso!");
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
                    UnidadeTempo = notificacaoNova.UnidadeTempo
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
