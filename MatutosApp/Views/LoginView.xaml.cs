using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class LoginView : ContentPage
{
	public LoginView(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}