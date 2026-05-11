using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
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
        public async Task<List<Servico>> ServicoConsultar()
        {
            return await _httpClient.GetFromJsonAsync<List<Servico>>("servico/consultar");
        }
    }
}
