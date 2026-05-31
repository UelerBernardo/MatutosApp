using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class PrincipalViewModel : BaseViewModel
    {

        //private readonly ClienteService _clienteService;

        private readonly UsuarioService _usuarioService;

        [ObservableProperty] private Usuario usuarioPerfil;

        public PrincipalViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _ = ConsultarPerfil();
        }

        public void AtualizarFotoTela()
        {
            OnPropertyChanged(nameof(FotoPerfilSource));
        }

        public ImageSource FotoPerfilSource
        {
            get
            {
                var usuarioLogado = UsuarioSessaoService.UsuarioLogado;

                return usuarioLogado == null || string.IsNullOrEmpty(usuarioLogado.Imagem_Usuario)
                    ? "user_placeholder.png"
                    : ImageSource.FromUri(new Uri($"https://localhost:7110{usuarioLogado.Imagem_Usuario}"));
            }
        }


        [RelayCommand]
        public async Task AbrirServico()
        {
            await Shell.Current.GoToAsync(nameof(ServicoConsultarView));
        }


        [RelayCommand]
        public async Task AbrirAgendamento()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoCadastroView));
        }

        [RelayCommand]
        public async Task MeusAgendamentos()
        {
            await Shell.Current.GoToAsync(nameof(AgendamentoConsultarView));
        }

        [RelayCommand]
        public async Task PerfilAbrir()
        {
            await Shell.Current.GoToAsync(nameof(ClientePerfilConsultarView));
        }

        public async Task ConsultarPerfil()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("jwt_token");

                var resultado = await _usuarioService.ConsultarPefil(token);

                if (resultado != null)
                {
                    UsuarioPerfil = resultado;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", $"Falha ao carregar perfil: {ex.Message}", "Ok");
            }
        }

        [RelayCommand]
        private async Task AbrirLocalizacao(string url)
        {
            try
            {
                url = "https://maps.google.com/maps/place//data=!4m2!3m1!1s0x94bbe7bb39cd6605:0xe5c2266ebd46985c?entry=s&sa=X&ved=2ahUKEwjgjd6Q7OGUAxVrCLkGHavtPIUQ4kB6BAgVEAA&hl=pt";
                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        url = $"https://{url}";
                    }
                    await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
                }
            }
            catch (Exception ex)
            {
                // Cai aqui se o celular do usuário não tiver nenhum navegador instalado
                await App.Current.MainPage.DisplayAlert("Erro", "Não foi possível abrir o link.", "OK");
            }
        }
    }
}
