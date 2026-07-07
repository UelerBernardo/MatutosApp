using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(EmailRecuperacao), "EmailDigitado")] 
    public partial class UsuarioRedefinirSenhaViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;

        [ObservableProperty] private string emailRecuperacao;
        [ObservableProperty] private string codigoRecuperacao;
        [ObservableProperty] private string senhaNova;

        public UsuarioRedefinirSenhaViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [RelayCommand]
        public async Task RedefinirSenha()
        {
            try
            {
                if (EmailRecuperacao.IsNullOrEmpty() || CodigoRecuperacao.IsNullOrEmpty() || SenhaNova.IsNullOrEmpty())
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha todos os campos!", "Ok");
                    return;
                }

                var resultado = await _usuarioService.RedefinirSenha(EmailRecuperacao, SenhaNova, CodigoRecuperacao);

                if (resultado.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "Ok");
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resultado.Mensagem, "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task SolicitarCodigo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EmailRecuperacao))
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha o e-mail para solicitação do código de autenticação.", "Ok");
                    return;
                }

                var resposta = await _usuarioService.SolicitarCodigo(EmailRecuperacao);

                if (resposta.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Código para redefinição reenviado.", "Ok");
                    return;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", resposta.Mensagem, "Ok");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro Crítico", $"Falha de comunicação: {ex.Message}", "OK");
            }
        }

    }
}
