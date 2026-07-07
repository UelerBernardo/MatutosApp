using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class BlacklistCadastrarView : ContentPage
{
	public BlacklistCadastrarView(BlacklistCadastrarViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}