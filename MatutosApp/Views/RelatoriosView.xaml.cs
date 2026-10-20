using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class RelatoriosView : ContentPage
{
	public RelatoriosView(RelatoriosViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}