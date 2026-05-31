using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MatutosApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.ViewsModels
{
    [QueryProperty(nameof(FotoPerfilCaminho), "FotoAtual")]
    public partial class UsuarioImagemViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FotoPerfilSource))]
        private string fotoPerfilCaminho;

        public ImageSource FotoPerfilSource => string.IsNullOrEmpty(FotoPerfilCaminho)
            ? "user_placeholder.png"
            : ImageSource.FromUri(new Uri($"https://localhost:7110{FotoPerfilCaminho}"));

        UsuarioService _usuarioService;

        public UsuarioImagemViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

        }

        [RelayCommand]
        public async Task EscolherImagem()
        {
            try
            {
                FileResult fotoEscolhida = await MediaPicker.Default.PickPhotoAsync();

                if (fotoEscolhida != null)
                {
                    string token = await SecureStorage.Default.GetAsync("jwt_token");

                    var resultado = await _usuarioService.CadastrarImagemUsuario(fotoEscolhida, token);

                    if (resultado.Sucesso)
                    {
                        UsuarioSessaoService.UsuarioLogado.Imagem_Usuario = resultado.CaminhoImagem;

                        FotoPerfilCaminho = resultado.CaminhoImagem;

                        await Application.Current.MainPage.DisplayAlert("Sucesso", resultado.Mensagem, "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Erro", resultado.Mensagem, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível acessar a galeria.", "OK");
            }
        }

        [RelayCommand]
        public async Task Cancelar()
        {
            if (Application.Current?.MainPage is Shell shell)
            {
                await shell.GoToAsync("..");
            }
            else if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

    }
}
