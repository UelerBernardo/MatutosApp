using Azure;
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
    public partial class ServicoService
    {
        private readonly HttpClient _httpClient;

        public ServicoService(HttpClient httpClient)
        {
            string baseURL = "https://localhost:7110/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };
        }

        //public async Task<(bool Sucesso, string Mensagem)> CadastrarImagemServico(string token, int codigoServico, Servico_Imagem _Imagem)
        //{
        //    try
        //    {

        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //        var request = _Imagem.Imagem.Conve

        //        var resposta = await _httpClient.PostAsync($"servico/{codigoServico}/imagens", );
        //    }
        //}
        public async Task<(bool Sucesso, string Mensagem)> CadastrarImagemServico(string token, int codigoServico, string imagemBase64)
        {
            try
            {
             
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                var requestBody = new
                {
                    ImagemBase64 = imagemBase64
                };

   
                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = $"/api/servico/{codigoServico}/imagens";

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Imagem salva com sucesso!");
                }
                else
                {
                    var erroResult = await response.Content.ReadAsStringAsync();
                    return (false, erroResult);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro de conexão ao enviar imagem: {ex.Message}");
            }
        }

        public async Task<(bool Sucesso, string Mensagem, List<Servico> Dados)> Consultar(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resposta = await _httpClient.GetAsync("servico/consultar");

                if (resposta.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = await resposta.Content.ReadFromJsonAsync<List<Servico>>(options);

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


        public async Task<List<Servico>> ServicoConsultar()
        {
            return await _httpClient.GetFromJsonAsync<List<Servico>>("servico/consultar");
        }

        public async Task<(bool Sucesso, string Mensagem)> ServicoCadastrar(Servico servico, string token)
        {
            try
            { 
                var tokenLimpo = token.Replace("Bearer ", "").Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenLimpo);

                var resposta = await _httpClient.PostAsJsonAsync<Servico>("servico/cadastrar", servico);

                if (resposta.IsSuccessStatusCode)
                {
                    return (true, "Serviço cadastrado com sucesso!");
                }

                else
                {
                    var errorMessage = await resposta.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao cadastrar pessoa. Status: {resposta.StatusCode}, Erro: {errorMessage}");
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar pessoa: {ex.Message}");
                return (false, string.Empty);
            }
        }
    }
}
