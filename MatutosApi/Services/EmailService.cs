using System.Net.Mail;
using System.Net;
using static MatutosApi.Controllers.UsuarioController;

namespace MatutosApi.Services
{
    public class EmailService : IEmailService
    {
        public async Task EnviarEmailRecuperacaoAsync(string emailDestino, string codigo)
        {
            string remetente = "barbeariamatutos@gmail.com";
            string senhaRemetente = "kgrl uixq gbfy upic";

            using (var clienteSmtp = new SmtpClient("smtp.gmail.com", 587))
            {
                clienteSmtp.EnableSsl = true;
                clienteSmtp.Credentials = new NetworkCredential(remetente, senhaRemetente);

                var mensagemDeEmail = new MailMessage(remetente, emailDestino)
                {
                    Subject = "Barbearia Matutos - Código de Recuperação de Senha",
                    Body = $"Olá! Seu código para redefinir a senha é: {codigo}. \nEste código expira em 15 minutos.",
                    IsBodyHtml = false
                };

                await clienteSmtp.SendMailAsync(mensagemDeEmail);
            }
        }
    }
}