using MatutosApp.Services;
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

            //ViewModels
            builder.Services.AddTransient<UsuarioViewModel>();
            builder.Services.AddTransient<TelefoneViewModel>();

            //Views
            builder.Services.AddTransient<UsuarioCadastroView>();
            builder.Services.AddTransient<TelefoneCadastroView>();
            //builder.Services.AddTransient<pgLoginView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
