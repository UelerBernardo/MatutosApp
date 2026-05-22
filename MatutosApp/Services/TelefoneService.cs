using Azure;
using MatutosDomain;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        public async Task<(bool Sucesso, string Mensagem)> TelefoneExcluir(string token, int codigoTelefone)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.DeleteAsync($"telefone/excluir/{codigoTelefone}");

                if(resposta.IsSuccessStatusCode)
                {
                    return (true, "Telefone excluído com sucesso!");
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();

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

        public async Task<(bool Sucesso, string Mensagem)> TelefoneAlterar(Telefone telefone, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resposta = await _httpClient.PutAsJsonAsync($"telefone/alterar/{telefone.Codigo_Telefone}", telefone);

                if(resposta.IsSuccessStatusCode)
                {
                    return (true, "Telefone alterado com Sucesso!");
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();

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

        public async Task<(bool Sucesso, string Mensagem )> TelefoneCadastrar(Telefone telefone, string tokenJwt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJwt);
                var resposta = await _httpClient.PostAsJsonAsync("telefone/cadastrar", telefone);

                if (resposta.IsSuccessStatusCode)
                {
                    var telefoneNovo = await resposta.Content.ReadFromJsonAsync<Telefone>();
                    return (true, string.Empty);
                }
                else
                {
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>();

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

        public async Task<(bool Sucesso, string Mensagem, List<UsuarioTelefone> Dados)> TelefoneConsultar(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.GetAsync("telefone/consultar");

                if(resposta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<List<UsuarioTelefone>>(options);

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
                return (false, "Falha de comunicação com o servidor. Verifique sua conexão com a internet.", null);
            }
        }

    }
}
