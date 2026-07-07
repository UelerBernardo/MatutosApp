using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class UsuarioRedefinirSenhaView : ContentPage
{
	public UsuarioRedefinirSenhaView(UsuarioRedefinirSenhaViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}