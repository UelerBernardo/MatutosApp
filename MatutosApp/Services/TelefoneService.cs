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

        //public async Task<bool> TelefoneCadastrar(Telefone telefone, string tokenJwt)
        //{
        //    try
        //    {
        //        // Limpeza de segurança: remove espaços ou aspas que podem vir no token
        //        var tokenLimpo = tokenJwt.Replace("Bearer ", "").Trim();

        //        // Criamos a requisição manualmente para ter controle total
        //        var request = new HttpRequestMessage(HttpMethod.Post, "telefone/cadastrar");

        //        // Injetamos o cabeçalho de autorização de forma explícita
        //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenLimpo);

        //        // Adicionamos o corpo da mensagem (o objeto telefone)
        //        request.Content = JsonContent.Create(telefone);

        //        // Enviamos
        //        var response = await _httpClient.SendAsync(request);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            var erro = await response.Content.ReadAsStringAsync();
        //            Debug.WriteLine($"Erro 401 ou 400: {erro}");
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Falha na comunicação: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> TelefoneCadastrar(Telefone telefone, string tokenJwt)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJwt);
                var response = await _httpClient.PostAsJsonAsync("telefone/cadastrar", telefone);

                if (response.IsSuccessStatusCode)
                {
                    var telefoneNovo = await response.Content.ReadFromJsonAsync<Telefone>();
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Falha ao cadastrar telefone. Status: {response.StatusCode}, Erro: {errorMessage}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exceção ao cadastrar pessoa: {ex.Message}");
                return false;
            }

        }

    }
}
