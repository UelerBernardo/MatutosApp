using Azure;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class TelefoneService
    {
        private readonly HttpClient _httpClient;

        public TelefoneService()
        {
            string baseURL = "https://localhost:7110/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };
        }

        public async Task<(bool Sucesso, string Mensagem )> TelefoneCadastrar(Telefone telefone, string tokenJwt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJwt);
                var response = await _httpClient.PostAsJsonAsync("telefone/cadastrar", telefone);

                if (response.IsSuccessStatusCode)
                {
                    var telefoneNovo = await response.Content.ReadFromJsonAsync<Telefone>();
                    return (true, string.Empty);
                }
                else
                {
                    var erroResposta = await response.Content.ReadFromJsonAsync<ApiErroResposta>();

                    string mensagemDaApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar requisição.";

                    Debug.WriteLine($"Falha: {mensagemDaApi}");
                    return (false, mensagemDaApi);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }

        }

    }
}
