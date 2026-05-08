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

        public async Task<(bool Sucesso, string Mensagem, int IdAgendamento)> AgendamentoServicoCadastrar(List<Agendamento_Servico> agendamento_Servico, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PostAsJsonAsync("agendamento/cadastrar/agendamentoServico", agendamento_Servico);

                if(resposta.IsSuccessStatusCode)
                {
                    var agendamentoNovo = await resposta.Content.ReadFromJsonAsync<Agendamento>();
                    return (true, string.Empty, agendamentoNovo.Codigo_Agendamento);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();
                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi,0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.",0);
            }

        }

        public async Task<(bool Sucesso, string Mensagem, int NovoId)> AgendamentoCadastrar(Agendamento agendamento, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PostAsJsonAsync("agendamento/cadastrar", agendamento);

                if(resposta.IsSuccessStatusCode)
                {
                    var agendamentoNovo = await resposta.Content.ReadFromJsonAsync<Agendamento>();
                    return (true, string.Empty, agendamentoNovo?.Codigo_Agendamento ?? 0);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();
                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha: {mensagemApi}");

                    return (false, mensagemApi, 0);
                }
            }

            catch(Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", 0);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> AgendamentoInativar(int codigoAgendamento, string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var conteudoVazio = new StringContent("{}", Encoding.UTF8, "application/json");

                var resposta = await _httpClient.PatchAsync($"agendamento/inativarAgendamento/{codigoAgendamento}", conteudoVazio);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Agendamento cancelado com sucesso.");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao inativar agendamento: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.");
            }
        }

       public async Task<(bool Sucesso, string Mensagem, Agendamento? Dados)> AgendamentoConsultarDetalhes(int codigoAgendamento, string token)
        {
            try
            {
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.GetAsync($"agendamento/consultar/{codigoAgendamento}");

               // var agendamento = await resposta.Content.ReadFromJsonAsync<Agendamento>();

                if (resposta.IsSuccessStatusCode)
                {

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<Agendamento>(options);

                    return (true, string.Empty, dados);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, mensagemApi, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao consultar detalhes: {ex.Message}");
                return (false, "Falha de comunicação com o servidor.", null);
            }
        }
    }
}
