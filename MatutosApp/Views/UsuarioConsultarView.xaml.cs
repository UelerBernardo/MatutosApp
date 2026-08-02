using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class UsuarioConsultarView : ContentPage
{
	public UsuarioConsultarView( UsuarioConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}