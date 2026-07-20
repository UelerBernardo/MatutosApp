using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosDomain;
using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public readonly UsuarioService _usuarioService;

        [ObservableProperty] private string senha;
        [ObservableProperty] private string email;

        public LoginViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }


        [RelayCommand]
        private async Task Logar()
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos.", "OK");
                return;
            }
            await LoginUsuario();

        }

        private async Task LoginUsuario()
        {
            try
            {
                var login = new UsuarioLogin
                {
                    Email = Email,
                    Senha = Senha
                };
                var resultado = await _usuarioService.UsuarioLogin(login);

                if (resultado.Sucesso)
                {
                    UsuarioSessaoService.IniciarSessao(resultado.Dados);

                    await _usuarioService.RegistrarTokenFCMAsync(resultado.TokenFCM);

                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Seja Bem-vindo!", "Ok");

                    await SolicitarPermissaoNotificacao();

                    await Shell.Current.GoToAsync("///PrincipalView");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task AbrirTela(string nomeDaRota)
        {
            // Navega para a tela solicitada
            await Shell.Current.GoToAsync(nomeDaRota);
        }

        public async Task SolicitarPermissaoNotificacao()
        {
            // A regra só se aplica se o aparelho for Android e versão 13 ou superior
            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 13)
            {
                // Verifica se o usuário já deu permissão antes
                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

                if (status != PermissionStatus.Granted)
                {
                    // Se não deu, sobe o pop-up nativo do Android perguntando!
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
                }
            }
        }
    }
}
