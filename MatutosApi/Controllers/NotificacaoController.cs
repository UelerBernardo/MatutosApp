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
