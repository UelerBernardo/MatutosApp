using FirebaseAdmin.Messaging;
using System;
using System.Threading.Tasks;

namespace MatutosApi.Services
{
    public class FirebaseService
    {
        public async Task<bool> EnviarPushNotificationAsync(string tokenFcmCliente, string titulo, string corpoMensagem)
        {
            if (string.IsNullOrWhiteSpace(tokenFcmCliente)) return false;

            try
            {
                // Monta a estrutura da mensagem que o Google exige
                var message = new Message()
                {
                    Token = tokenFcmCliente,
                    Notification = new Notification()
                    {
                        Title = titulo,
                        Body = corpoMensagem
                    },
                    // 👉 Força o Android a dar atenção imediata à mensagem
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "default"
                        }
                    }
                };

                // Envia de fato para os servidores do Firebase
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                // Se retornou o ID da mensagem, deu certo!
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                // Loga o erro caso o token seja inválido ou expirado
                Console.WriteLine($"Erro ao enviar Push via Firebase: {ex.Message}");
                return false;
            }
        }
    }
}