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
                                // 1. Define a janela de tempo baseada no parâmetro do banco (ex: 20 minutos)
                                // Se for UnidadeTempoEnum.Minutos, somamos os minutos. 

                                var agora = DateTime.Now;
                                // Zera os segundos para garantir a correspondência exata do minuto
                                var agoraSemSegundos = new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, 0);

                                var inicioJanela = agoraSemSegundos.AddMinutes(regra.Valor.Value);
                                var fimJanela = inicioJanela.AddMinutes(1);

                                // 2. Busca os agendamentos que vão acontecer dentro dessa janela
                                // OBS: Ajuste os nomes "DataHora" e "Status" para os campos reais da sua tabela de Agendamento
                                var agendamentosProximos = await dbContext.Agendamentos
                                    .Where(a => a.Data_Agendamento >= inicioJanela &&
                                               a.Data_Agendamento < fimJanela &&
                                               a.Codigo_Situacao_Agendamento == AgendamentoSituacao.Liberado) // Só avisa quem está confirmado
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
                                                regra.Mensagem   // Corpo do push
                                            );

                                            if (pushEnviado)
                                                _logger.LogInformation($"Push enviado com sucesso para o cliente {agendamento.Codigo_Cliente}.");
                                            else
                                                _logger.LogWarning($"Falha ao entregar push para o cliente {agendamento.Codigo_Cliente}.");
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Cliente {agendamento.Codigo_Cliente} não possui um dispositivo (Token FCM) registrado.");
                                        }
                                    }
                                }
                            }

                            // Espaço reservado para o Gatilho 2 (Inatividade) no futuro...
                            // else if (regra.Codigo_Tipo == 2) { ... }
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
