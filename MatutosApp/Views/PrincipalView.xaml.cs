using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class PrincipalView : ContentPage
{
	public PrincipalView(PrincipalViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}