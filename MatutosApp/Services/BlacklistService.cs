using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Views;
using MatutosDomain;
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
    public partial class BlacklistService
    {
        private readonly HttpClient _httpClient;

        public BlacklistService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            //string baseURL = DeviceInfo.Platform == DevicePlatform.Android
            //    ? "https://10.0.2.2:7110/" // 👉 Emulador acessando a máquina (HTTPS)
            //    : "https://localhost:7110/";

        }
        public async Task<(bool Sucesso, string Mensagem)> AlterarBlacklist(Blacklist blacklist, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resultado = await _httpClient.PutAsJsonAsync("blacklist/alterar", blacklist);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if(resultado.IsSuccessStatusCode)
                {
                    var retornoApi = await resultado.Content.ReadFromJsonAsync<ApiResposta>(options);

                    return (true, retornoApi.Mensagem ?? "Bloqueio alterado com sucesso!");
                }
                else
                {
                    var retornoApi = await resultado.Content.ReadFromJsonAsync<ApiResposta>(options);

                    return (false, retornoApi.Mensagem ?? "Exceção para alterar a regra de bloqueio!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar blacklist: {ex.Message}");
                return (false, string.Empty);
            }
        }

        public async Task<(bool Sucesso, string Mensagem)> CadastrarBlacklist(Blacklist blacklist, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.PostAsJsonAsync<Blacklist>("blacklist/cadastrar", blacklist);

                if(resposta.IsSuccessStatusCode)
                {
                    return (true, "Bloqueio cadastrado com sucesso");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resposta.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha ao cadastrar blacklist. Status: {resposta.StatusCode}, Erro: {mensagemApi}");

                    return (false, mensagemApi);
                }
            }
             
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar blacklist: {ex.Message}");
                return (false, string.Empty);
            }
        }

        public async Task<(bool Sucesso, List<BlacklistResponse> Dados, string Mensagem)> ConsultarBlacklist(string token, DateTime inicial, DateTime final)
        {
            try
            {
                var query = new List<string>
                {
                    $"inicial={inicial:yyyy-MM-ddTHH:mm:ss}",
                    $"final={final:yyyy-MM-ddTHH:mm:ss}"
                };

                string queryString = string.Join("&", query);

                string urlCompleta = $"blacklist/consultar?{queryString}";

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resultado = await _httpClient.GetAsync(urlCompleta);

                if (resultado.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // 👉 Lemos o JSON usando o novo formato que bate 100% com a API
                    var dados = await resultado.Content.ReadFromJsonAsync<List<BlacklistResponse>>(options);

                     return (true, dados, string.Empty);
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var erroResposta = await resultado.Content.ReadFromJsonAsync<ApiErroResposta>(options);

                    string mensagemApi = erroResposta?.Mensagem ?? "Erro desconhecido ao processar a requisição.";
                    Debug.WriteLine($"Falha na API: {mensagemApi}");

                    return (false, null, mensagemApi);
                }
            }
            catch (Exception ex)
            {
                // 👉 Corrigido o erro de digitação do copia e cola e adicionando retorno da mensagem para a UI!
                Debug.WriteLine($"Exceção ao consultar Blacklist: {ex.Message}");
                return (false, null, "Erro de comunicação com o servidor.");
            }
        }
    }
}
