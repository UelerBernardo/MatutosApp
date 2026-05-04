using MatutosDomain;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public class AgendamentoService
    {
        private readonly HttpClient _httpClient;

        public AgendamentoService()
        {
            string baseURL = "https://localhost:7110/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };
        }

        public async Task<(bool Sucesso, string Mensagem)> AgendamentoCadastrar(Agendamento agendamento, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PostAsJsonAsync("agendamento/cadastrar", agendamento);

                if(resposta.IsSuccessStatusCode)
                {
                    var agendamentoNovo = await resposta.Content.ReadFromJsonAsync<Agendamento>();
                    return (true, string.Empty);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();
                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }

            catch(Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }
    }
}
