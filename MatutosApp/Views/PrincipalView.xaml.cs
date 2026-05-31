using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class PrincipalView : ContentPage
{
	public PrincipalView(PrincipalViewModel viewModel)
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