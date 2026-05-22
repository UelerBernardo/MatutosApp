using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ClientePerfilConsultarView : ContentPage
{
	public ClientePerfilConsultarView(ClientePerfilConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ClientePerfilConsultarViewModel viewModel)
        {
            viewModel.AtualizarFotoTela();
        }
    }
}