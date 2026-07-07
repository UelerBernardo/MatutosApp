using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using MatutosApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    public partial class UsuarioSolicitarCodigoViewModel : BaseViewModel
    {
        private readonly UsuarioService _usuarioService;

        [ObservableProperty] string email;

        public UsuarioSolicitarCodigoViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [RelayCommand]
        public async Task SolicitarCodigo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    await Application.Current.MainPage.DisplayAlert("Atenção", "Por favor preencha o e-mail para solicitação do código de autenticação.", "Ok");
                    return;
                }

                var resposta = await _usuarioService.SolicitarCodigo(Email);

                if (resposta.Sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", resposta.Mensagem, "Ok");

                    // Ajuste 2: Cria um pacote com o e-mail para mandar para a próxima tela
                    var parametros = new Dictionary<string, object>
                    {
                        { "EmailDigitado", Email }
                    };
                    await Shell.Current.GoToAsync(nameof(UsuarioRedefinirSenhaView), parametros);
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
