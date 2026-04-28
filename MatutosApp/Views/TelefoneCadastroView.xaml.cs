using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class TelefoneCadastroView : ContentPage
{
	public TelefoneCadastroView(TelefoneViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}