using Microsoft.Maui; // Resolve o IPropertyMapper (AppendToMapping)
using Microsoft.Maui.Hosting; // Resolve o MauiAppBuilder
using Microsoft.Maui.Controls.Hosting; // 👉 Resolve o UseMauiApp
using Microsoft.Extensions.DependencyInjection; // 👉 Resolve o AddHttpClient
using Microsoft.Maui.Devices;                   // 👉 Resolve o DeviceInfo e DevicePlatform
using Microsoft.Maui.Controls;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosApp.ViewsModels;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.Bundled.Shared;
using Microsoft.Maui.LifecycleEvents;
#if ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
#endif

namespace MatutosApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android.OnCreate((activity, state) =>
                    {
                        // 👉 Na versão 3.x, passamos apenas o activity e as configurações!
                        CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings());
                    }));
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string ipDaApi = DeviceInfo.Platform == DevicePlatform.Android
      ? "https://10.0.2.2:7110/"
      : "https://localhost:7110/";



            // 👉 2. O passaporte de segurança do emulador
#if ANDROID
            var devSslHandler = new HttpClientHandler();
            devSslHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            // 👉 3. Todas as services registradas recebendo a rede e a segurança!
            builder.Services.AddHttpClient<UsuarioService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<BarbeiroService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<TelefoneService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<AgendamentoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<ServicoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<ClienteService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<BlacklistService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
            ;

            builder.Services.AddHttpClient<NotificacaoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
           .ConfigurePrimaryHttpMessageHandler(() => devSslHandler)
#endif
           ;



            // 👉 3. ViewModels
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
            builder.Services.AddTransient<UsuarioSolicitarCodigoViewModel>();
            builder.Services.AddTransient<UsuarioRedefinirSenhaViewModel>();
            builder.Services.AddTransient<ConfiguraNotificacaoCadastrarViewModel>();
            builder.Services.AddTransient<ConfiguraNotificacaoConsultarViewModel>();
            builder.Services.AddTransient<NotificacaoConsultarViewModel>();
            builder.Services.AddTransient<UsuarioConsultarViewModel>();


            // 👉 4. Views
            builder.Services.AddTransient<UsuarioCadastroView>();
            builder.Services.AddTransient<TelefoneCadastroView>();
            builder.Services.AddTransient<BlacklistConsultarView>();
            builder.Services.AddTransient<AgendamentoDetalhesView>();
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
            builder.Services.AddTransient<UsuarioSolicitarCodigoView>();
            builder.Services.AddTransient<UsuarioRedefinirSenhaView>();
            builder.Services.AddTransient<ConfiguraNotificacaoCadastrarView>();
            builder.Services.AddTransient<ConfiguraNotificacaoConsultarView>();
            builder.Services.AddTransient<NotificacaoConsultarView>();
            builder.Services.AddTransient<UsuarioConsultarView>();


            // 👉 5. Configuração Visual (Somente regra de interface aqui dentro!)
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SemBorda", (handler, view) =>
            {
                if (handler.PlatformView == null) return;

#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
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
        private static CrossFirebaseSettings CreateCrossFirebaseSettings()
        {
            return new CrossFirebaseSettings(
                isAuthEnabled: false,
                isCloudMessagingEnabled: true,
                isAnalyticsEnabled: false,
                isCrashlyticsEnabled: false
                );
        }
    }
}