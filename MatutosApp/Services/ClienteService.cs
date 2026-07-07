using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class ClienteService
    {
        private readonly HttpClient _httpClient;

        public ClienteService(HttpClient httpClient)
        {
            string baseURL = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7110/" // 👉 Emulador acessando a máquina (HTTPS)
                : "https://localhost:7110/"; // 👉 Windows acessando a máquina (HTTPS)

            _httpClient = new HttpClient(ObterManipuladorInseguro())
            {
                BaseAddress = new Uri(baseURL)
            };

        }

        private HttpMessageHandler ObterManipuladorInseguro()
        {
            #if ANDROID
                var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert != null && cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };
                return handler;
            #else
                        return new HttpClientHandler();
            #endif
        }

        public async Task<Cliente> ClienteConsultarPefil(string token)
        {
            var tokenLimpo = token.Replace("Bearer ", "").Trim();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenLimpo);

            return await _httpClient.GetFromJsonAsync<Cliente>("cliente/consultar");
        }

        public async Task<(bool Sucesso, string Mensagem)> AlterarSenhaCliente(string token, string senhaAtualDigitada, string novaSenhaDigitada)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenLimpo);

                var pacote = new
                {
                    SenhaAntiga = senhaAtualDigitada,
                    SenhaNova = novaSenhaDigitada
                };

                var resposta = await _httpClient.PostAsJsonAsync("cliente/alterar/senha", pacote);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Senha alterada com sucesso!");
                }
                else
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    System.Diagnostics.Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exceção ao alterar senha: {ex.Message}");
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão com a internet.");
            }
        }
    }
}
