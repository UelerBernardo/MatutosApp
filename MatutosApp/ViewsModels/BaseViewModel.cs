using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MatutosApp.Services;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MatutosApp.ViewsModels
{
    public partial class BaseViewModel : BaseNotifyViewModel
    {
        public ICommand AbrirTela { get; }
        public ICommand VoltarTela { get; }


        public bool IsBarbeiro => UsuarioSessaoService.UsuarioLogado?.TipoSelecionado == UsuarioTipo.Barbeiro;

        public bool IsAdministrador => UsuarioSessaoService.UsuarioLogado?.TipoSelecionado == UsuarioTipo.Administrador;

        public bool IsCliente => UsuarioSessaoService.UsuarioLogado?.TipoSelecionado == UsuarioTipo.Cliente;

        public BaseViewModel()
        {
            VoltarTela = new AsyncRelayCommand(VoltarTelaAsync);
            AbrirTela = new AsyncRelayCommand<Type>(AbrirTelaAsync);

            WeakReferenceMessenger.Default.Register<string>(this, (r, mensagem) =>
            {
                if (mensagem == "SessaoAlterada")
                {
                    OnPropertyChanged(nameof(IsCliente));
                    OnPropertyChanged(nameof(IsBarbeiro));
                    OnPropertyChanged(nameof(IsAdministrador));
                }
            });
        }

        public async Task VoltarTelaAsync()
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

        public async Task AbrirTelaAsync(Type pageType)
        {
            if (pageType == null)
            {
                Debug.WriteLine("Erro: Tipo de página para navegação é nulo.");
                return;
            }

            try
            {
                if (Application.Current?.MainPage is Shell shell)
                {
                    // Usa o nome da rota registrado no AppShell
                    await shell.GoToAsync(pageType.Name);
                }
                else
                {
                    var page = App.Current.Handler.MauiContext.Services.GetService(pageType) as ContentPage;
                    if (page != null)
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(page);
                    }
                    else
                    {
                        Debug.WriteLine($"Erro: Não foi possível resolver a página do tipo {pageType.Name}. Verifique o registro no MauiProgram.cs.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao navegar para {pageType.Name}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task FazerLogoutAsync()
        {
            bool confirmar = await Shell.Current.DisplayAlert("Sair",
                                                              "Deseja realmente sair do sistema?",
                                                              "Sim", "Não");
            if (!confirmar)
                return;
            UsuarioSessaoService.EncerrarSessao();

        }

        [ObservableProperty]
        private bool isBusy;

        // BÔNUS: É muito útil ter uma propriedade de Título na Base também!
        // Assim, qualquer ViewModel pode alterar o título da página facilmente.
        [ObservableProperty]
        private string title;
    }
}
