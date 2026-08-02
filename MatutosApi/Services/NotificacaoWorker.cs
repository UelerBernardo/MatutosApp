using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MatutosApi.Infraestrutura;
using MatutosDomain;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatutosApi.Services
{
    public class NotificacaoWorker : BackgroundService
    {
        private readonly ILogger<NotificacaoWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FirebaseService _firebaseService; 

        // Define de quanto em quanto tempo o robô vai rodar. 
        // Para testes, deixei 1 minuto. No mundo real, poderia ser 5 ou 10 minutos.
        private readonly TimeSpan _intervaloExecucao = TimeSpan.FromMinutes(1);

        public NotificacaoWorker(ILogger<NotificacaoWorker> logger, IServiceScopeFactory scopeFactory, FirebaseService firebaseService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _firebaseService = firebaseService; // Injetado com sucesso!
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("O motor de Notificações da Barbearia Matutos foi iniciado.");

            // Usando o PeriodicTimer moderno do .NET para controlar o loop sem travar a thread
            using var timer = new PeriodicTimer(_intervaloExecucao);

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Executando ciclo de verificação de regras de notificação: {Tempo}", DateTime.Now);

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MatutosDbContext>();

                        var regrasAtivas = await dbContext.Configura_Notificacoes
                                                          .Where(r => r.Ativo)
                                                          .ToListAsync(stoppingToken);

                        if (!regrasAtivas.Any())
                        {
                            _logger.LogInformation("Nenhuma regra ativa encontrada.");
                            continue;
                        }

                        foreach (var regra in regrasAtivas)
                        {
                            _logger.LogInformation("Processando Regra ID: {Id} - {Descricao}", regra.Codigo_Notificacao, regra.Descricao);

                            // 👉 GATILHO 1: PRÉ-AGENDAMENTO
                            if (regra.Codigo_Tipo == 1 && regra.Valor.HasValue)
                            {
                                var agora = DateTime.Now;
                                // Zera os segundos para garantir a correspondência exata do minuto
                                var agoraSemSegundos = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, 0);

                                // 1. Declara as variáveis (Tipando explicitamente como DateTime)
                                DateTime inicioJanela = agoraSemSegundos;
                                DateTime fimJanela = agoraSemSegundos;

                                // 2. Atualiza as variáveis de acordo com a unidade, SEM usar 'var' novamente
                                if (regra.UnidadeTempo == UnidadeTempoEnum.Dias)
                                {
                                    inicioJanela = agoraSemSegundos.AddDays(regra.Valor.Value);
                                    fimJanela = inicioJanela.AddMinutes(1);
                                }
                                else if (regra.UnidadeTempo == UnidadeTempoEnum.Minutos)
                                {
                                    inicioJanela = agoraSemSegundos.AddMinutes(regra.Valor.Value);
                                    fimJanela = inicioJanela.AddMinutes(1);
                                }
                                else if (regra.UnidadeTempo == UnidadeTempoEnum.Horas)
                                {
                                    inicioJanela = agoraSemSegundos.AddHours(regra.Valor.Value);
                                    fimJanela = inicioJanela.AddMinutes(1);
                                }

                                // 3. Busca os agendamentos usando as janelas calculadas
                                var agendamentosProximos = await dbContext.Agendamentos
                                        .Where(a => a.Data_Agendamento >= inicioJanela &&
                                                    a.Data_Agendamento < fimJanela &&
                                                    a.Codigo_Situacao_Agendamento == AgendamentoSituacao.Liberado)
                                        .ToListAsync(stoppingToken);

                                foreach (var agendamento in agendamentosProximos)
                                {
                                    // 3. Verificação Anti-Spam: Já geramos essa notificação para esse agendamento?
                                    bool jaNotificado = await dbContext.Notificacoes
                                        .AnyAsync(n => n.Codigo_Notificacao == regra.Codigo_Notificacao &&
                                                       n.Codigo_Usuario == agendamento.Codigo_Cliente, stoppingToken); // Ajuste as chaves se necessário

                                    var clienteNotificacao = await dbContext.Usuarios
                                        .Where(u => u.Codigo_Usuario == agendamento.Codigo_Cliente).FirstOrDefaultAsync();

                                    string nomeCompleto = clienteNotificacao.Nome ?? "Cliente";
                                    string primeiroNome = nomeCompleto.Trim().Split(' ').FirstOrDefault() ?? "Cliente";

                                    if (!jaNotificado)
                                    {
                                        // 1. Cria e adiciona a notificação no histórico do banco de dados
                                        var novaNotificacao = new Notificacao
                                        {
                                            Codigo_Notificacao = regra.Codigo_Notificacao,
                                            Codigo_Usuario = agendamento.Codigo_Cliente,
                                            Codigo_Agendamento  = agendamento.Codigo_Agendamento,
                                            MensagemEnviada = $"Olá, {primeiroNome}! {regra.Mensagem}",
                                            DataDisparo = DateTime.Now,
                                            Lida = false
                                        };

                                        dbContext.Notificacoes.Add(novaNotificacao);

                                        // 2. DISPARO DO PUSH REAL: Busca o Token FCM do cliente
                                        // OBS: Aqui estou simulando que a tabela de cliente/usuário possui o campo TokenFCM
                                        var tokenCliente = await dbContext.Usuarios
                                            .Where(u => u.Codigo_Usuario == agendamento.Codigo_Cliente)
                                            .Select(u => u.TokenFCM)
                                            .FirstOrDefaultAsync(stoppingToken);

                                        if (!string.IsNullOrWhiteSpace(tokenCliente))
                                        {
                                            // Envia o push de forma assíncrona
                                            bool pushEnviado = await _firebaseService.EnviarPushNotificationAsync(
                                                tokenCliente,
                                                regra.Descricao, // Título do push (ex: "Aviso de Agendamento")
                                                novaNotificacao.MensagemEnviada  // Corpo do push
                                            );

                                            if (pushEnviado)
                                                _logger.LogInformation($"Push enviado com sucesso para o cliente {agendamento.Codigo_Cliente}.");

                                            if (!pushEnviado)
                                            {
                                                _logger.LogWarning($"Falha ao entregar push para o cliente {agendamento.Codigo_Cliente}.");

                                                // 👉 DICA DE OURO: Se o erro for "NotRegistered", limpe o token inválido do banco!
                                                // Isso evita que o sistema tente enviar para esse token "morto" nos próximos ciclos.
                                                var usuario = await dbContext.Usuarios.FindAsync(agendamento.Codigo_Cliente);
                                                if (usuario != null)
                                                {
                                                    usuario.TokenFCM = null;
                                                    await dbContext.SaveChangesAsync(stoppingToken);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Cliente {agendamento.Codigo_Cliente} não possui um dispositivo (Token FCM) registrado.");
                                        }
                                    }
                                }
                            }
                            else if(regra.Codigo_Tipo == 2 && regra.Valor.HasValue)
                            {
                                //Aqui pegamos a data atual sem os minutos e segundos
                                DateTime dataAtual = DateTime.Now.Date;

                                var dataValidacao = dataAtual.AddDays (- regra.Valor.Value);

                                var clientesNaoFidelizados = await dbContext.Clientes
                                    .Where(c => c.Ativo)
                                    .Select(c => new
                                    {
                                        Cliente = c,

                                        UltimoAgendamento = dbContext.Agendamentos
                                            .Where(a => a.Codigo_Cliente == c.Codigo_Usuario)
                                            .Max(a => (DateTime?)a.Data_Agendamento)
                                    })
                                    .Where(x => x.UltimoAgendamento == null || x.UltimoAgendamento <= dataValidacao).ToListAsync(stoppingToken);

                                foreach(var item in clientesNaoFidelizados)
                                {
                                    var cliente = item.Cliente;

                                    bool jaNotificado = await dbContext.Notificacoes
                                        .AnyAsync(n => n.Codigo_Notificacao == regra.Codigo_Notificacao &&
                                                       n.Codigo_Usuario == cliente.Codigo_Usuario, stoppingToken);

                                    if(!jaNotificado)
                                    {
                                        string nomeCompleto = cliente.Nome ?? "Cliente";
                                        string primeiroNome = nomeCompleto.Trim().Split(' ').FirstOrDefault() ?? "Cliente";

                                        var mensagemClienteSumido = new Notificacao
                                        {
                                            Codigo_Notificacao = regra.Codigo_Notificacao,
                                            Codigo_Usuario = cliente.Codigo_Usuario,
                                            Codigo_Agendamento = null,
                                            MensagemEnviada = $"Olá, {primeiroNome}! {regra.Mensagem}",
                                            DataDisparo = DateTime.Now,
                                            Lida = false
                                        };

                                        dbContext.Notificacoes.Add(mensagemClienteSumido);

                                        var tokenCliente = cliente.TokenFCM;

                                        if (!string.IsNullOrWhiteSpace(tokenCliente))
                                        {
                                            // Envia o push de forma assíncrona
                                            bool pushEnviado = await _firebaseService.EnviarPushNotificationAsync(
                                                tokenCliente,
                                                regra.Descricao,
                                                mensagemClienteSumido.MensagemEnviada
                                            );

                                            if (pushEnviado)
                                            {
                                                _logger.LogInformation($"Push enviado com sucesso para o cliente {cliente.Codigo_Usuario}.");
                                            }
                                            else
                                            {
                                                _logger.LogWarning($"Falha ao entregar push para o cliente {cliente.Codigo_Usuario}.");
                                                cliente.TokenFCM = null;
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Cliente {cliente.Codigo_Usuario} não possui um dispositivo (Token FCM) registrado.");
                                        }

                                    }
                                        
                                }

                            }
                            else if (regra.Codigo_Tipo == 3)
                            {
                                // Verifica se a regra já foi disparada alguma vez
                                var jaNotificado = await dbContext.Notificacoes
                                    .AnyAsync(n => n.Codigo_Notificacao == regra.Codigo_Notificacao, stoppingToken);

                                if (!jaNotificado)
                                {
                                    // 👉 OTIMIZAÇÃO: Busca Clientes e seus respectivos Usuários (Tokens) de uma vez só!
                                    var clientesComTokens = await dbContext.Clientes
                                        .Where(c => c.Ativo == true)
                                        .Join(dbContext.Usuarios,
                                              cliente => cliente.Codigo_Usuario,
                                              usuario => usuario.Codigo_Usuario,
                                              (cliente, usuario) => new { Cliente = cliente, Usuario = usuario })
                                        .ToListAsync(stoppingToken);

                                    foreach (var info in clientesComTokens)
                                    {
                                        string nomeCompleto = info.Cliente.Nome ?? "Cliente";
                                        string primeiroNome = nomeCompleto.Trim().Split(' ').FirstOrDefault() ?? "Cliente";

                                        // Cria o registro da notificação
                                        var mensagemPromocao = new Notificacao
                                        {
                                            Codigo_Notificacao = regra.Codigo_Notificacao,
                                            Codigo_Usuario = info.Cliente.Codigo_Usuario,
                                            Codigo_Agendamento = null,
                                            MensagemEnviada = $"Olá, {primeiroNome}! {regra.Mensagem}",
                                            DataDisparo = DateTime.Now,
                                            Lida = false
                                        };

                                        // Adiciona no rastreio do Entity Framework (ainda não salvou no banco)
                                        dbContext.Notificacoes.Add(mensagemPromocao);

                                        var tokenCliente = info.Usuario.TokenFCM;

                                        if (!string.IsNullOrWhiteSpace(tokenCliente))
                                        {
                                            // Envia o push de forma assíncrona
                                            bool pushEnviado = await _firebaseService.EnviarPushNotificationAsync(
                                                tokenCliente,
                                                regra.Descricao,
                                                mensagemPromocao.MensagemEnviada
                                            );

                                            if (pushEnviado)
                                            {
                                                _logger.LogInformation($"Push enviado com sucesso para o cliente {info.Cliente.Codigo_Usuario}.");
                                            }
                                            else
                                            {
                                                _logger.LogWarning($"Falha ao entregar push para o cliente {info.Cliente.Codigo_Usuario}.");
                                                info.Usuario.TokenFCM = null;
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Cliente {info.Cliente.Codigo_Usuario} não possui um dispositivo (Token FCM) registrado.");
                                        }
                                    }
                                }
                            }
                        }
                        // 5. Salva todas as notificações geradas neste ciclo de uma só vez (Performance)
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro crítico ao processar o ciclo de notificações.");
                }
            }

            _logger.LogInformation("O motor de Notificações SMR foi parado.");
        }
    }
}
