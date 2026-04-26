using MatutosApp.ViewsModels;
namespace MatutosApp.Views;

public partial class UsuarioCadastroView : ContentPage
{
	public UsuarioCadastroView(UsuarioViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}