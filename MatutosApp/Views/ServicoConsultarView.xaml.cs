using MatutosApp.ViewsModels;

namespace MatutosApp.Views;

public partial class ServicoConsultarView : ContentPage
{
	public ServicoConsultarView(ServicoConsultarViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ServicoConsultarViewModel viewModel)
        {
            viewModel.ConsultarServicoCommand.Execute(null);
        }
    }
}