using MatutosApp.ViewsModels;
using Microsoft.Maui.Controls;

namespace MatutosApp.Views
{
    public partial class AuditoriaLogDetalhesView : ContentPage
    {
        private readonly AuditoriaLogDetalhesViewModel _viewModel;

        public AuditoriaLogDetalhesView(AuditoriaLogDetalhesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            // Executa o comando de carregamento assim que navega para a página
            // O CommunityToolkit.Mvvm gera automaticamente o sufixo "Command" para métodos com [RelayCommand]
            await _viewModel.LogConsultarDetalhesCommand.ExecuteAsync(null);
        }
    }
}