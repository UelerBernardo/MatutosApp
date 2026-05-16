using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class AgendamentoConsultarView : ContentPage
{
	public AgendamentoConsultarView(AgendamentoConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}