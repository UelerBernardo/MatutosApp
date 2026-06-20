using MatutosApp.Services;
using Microsoft.Extensions.DependencyInjection;
using MatutosApp.Views;
using MatutosApp.ViewsModels;
using Microsoft.Extensions.Logging;

namespace MatutosApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //Serviços
            //builder.Services.AddSingleton<ApiServicesSessaoPessoa>();
            builder.Services.AddSingleton<UsuarioService>();
            builder.Services.AddSingleton<TelefoneService>();
            builder.Services.AddSingleton<AgendamentoService>();
            builder.Services.AddSingleton<ServicoService>();
            builder.Services.AddSingleton<ClienteService>();
            builder.Services.AddSingleton<BlacklistService>();
            builder.Services.AddHttpClient<BarbeiroService>(cliente =>
            {
               cliente.BaseAddress = new Uri("https://localhost:7110/"); 
            });

            //ViewModels
            builder.Services.AddTransient<UsuarioViewModel>();
            builder.Services.AddTransient<TelefoneViewModel>();
            builder.Services.AddTransient<AgendamentoViewModel>();
            builder.Services.AddTransient<PrincipalViewModel>();
            builder.Services.AddTransient<AgendamentoServicoViewModel>();
            builder.Services.AddTransient<AgendamentoDetalhesViewModel>();
            builder.Services.AddTransient<AgendamentoConsultarViewModel>();
            builder.Services.AddTransient<ClientePerfilConsultarViewModel>();
            builder.Services.AddTransient<UsuarioTelefoneConsultarViewModel>();
            builder.Services.AddTransient<UsuarioImagemViewModel>();
            builder.Services.AddTransient<ServicoConsultarViewModel>();
            builder.Services.AddTransient<ServicoCadastrarViewModel>();
            builder.Services.AddTransient<BlacklistConsultarViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<BlacklistCadastrarViewModel>();

            //Views
            builder.Services.AddTransient<UsuarioCadastroView>();
            builder.Services.AddTransient<TelefoneCadastroView>();
            builder.Services.AddTransient<BlacklistConsultarView>();
            builder.Services.AddTransient<AgendamentoDetalhesViewModel>();
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<PrincipalView>();
            builder.Services.AddTransient<AgendamentoCadastroView>();
            builder.Services.AddTransient<AgendamentoServicoView>();
            builder.Services.AddTransient<AgendamentoConsultarView>();
            builder.Services.AddTransient<ClientePerfilConsultarView>();
            builder.Services.AddTransient<UsuarioTelefoneConsultarView>();
            builder.Services.AddTransient<UsuarioImagemView>();
            builder.Services.AddTransient<ServicoConsultarView>();
            builder.Services.AddTransient<ServicoCadastroView>();
            builder.Services.AddTransient<BlacklistCadastrarView>();



            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SemBorda", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS
        handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
        handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        handler.PlatformView.FocusVisualMargin = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });




#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
