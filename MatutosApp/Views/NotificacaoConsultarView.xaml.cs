using MatutosApp.ViewsModels;
namespace MatutosApp.Views;

public partial class NotificacaoConsultarView : ContentPage
{
    private readonly NotificacaoConsultarViewModel _viewModel;
    public NotificacaoConsultarView(NotificacaoConsultarViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Executa a consulta automaticamente ao entrar na tela
        if (_viewModel.ConsultarNotificacaoCommand.CanExecute(null))
        {
            _viewModel.ConsultarNotificacaoCommand.Execute(null);
        }
    }
}