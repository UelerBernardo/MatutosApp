using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AgendamentoDetalhesView : ContentPage
{
	public AgendamentoDetalhesView(AgendamentoDetalhesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}