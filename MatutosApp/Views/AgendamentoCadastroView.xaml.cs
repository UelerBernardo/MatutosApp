using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AgendamentoCadastroView : ContentPage
{
	public AgendamentoCadastroView(AgendamentoViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}