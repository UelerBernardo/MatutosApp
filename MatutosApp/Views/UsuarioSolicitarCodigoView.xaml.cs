using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class UsuarioSolicitarCodigoView : ContentPage
{
	public UsuarioSolicitarCodigoView(UsuarioSolicitarCodigoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}