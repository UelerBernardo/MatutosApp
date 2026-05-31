using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ServicoCadastroView : ContentPage
{
	public ServicoCadastroView(ServicoCadastrarViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}