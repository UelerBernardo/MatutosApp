using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class UsuarioImagemView : ContentPage
{
	public UsuarioImagemView(UsuarioImagemViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}