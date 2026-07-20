using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ConfiguraNotificacaoConsultarView : ContentPage
{
	public ConfiguraNotificacaoConsultarView(ConfiguraNotificacaoConsultarViewModel viewModel )
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Pega a ViewModel injetada
        if (BindingContext is ConfiguraNotificacaoConsultarViewModel vm)
        {
            // Dispara o comando que vai na API buscar os dados
            vm.ConsultarRegraNotificacaoCommand.Execute(null);
        }
    }
}