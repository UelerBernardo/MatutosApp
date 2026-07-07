using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{ 
    public class BarbeiroService
    {
        private readonly HttpClient _httpClient;

        public BarbeiroService(HttpClient httpClient)
        {
            string baseURL = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7110/" // 👉 Emulador acessando a máquina (HTTPS)
            : "https://localhost:7110/";

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

        public async Task<List<Barbeiro>> BarbeiroConsultar()
        {
            return await _httpClient.GetFromJsonAsync<List<Barbeiro>>("barbeiro/consultar");
        }
    }
}
