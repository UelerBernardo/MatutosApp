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

                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Seja Bem-vindo!", "Ok");

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
    }
}
