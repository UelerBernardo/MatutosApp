using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class LoginView : ContentPage
{
	public LoginView(UsuarioViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}