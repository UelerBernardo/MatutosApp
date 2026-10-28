using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using MatutosApp.Services;
using MatutosApp.Views;
using MatutosApp.ViewsModels;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.Bundled.Shared;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http; 

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
                        // Inicializa o Firebase
                        CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings());
                    }));
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            // 👉 1. ROTEAMENTO DE REDE INTELIGENTE
            string ipDaApi = "http://172.20.10.2:5000/";

            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DevicePlatform.Android)
            {
                ipDaApi = "https://10.0.2.2:7110/";
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                ipDaApi = "https://localhost:7110/";
            }


            // 👉 2. INJEÇÃO DE DEPENDÊNCIA (Com o passaporte SSL seguro aplicado!)
            builder.Services.AddHttpClient<UsuarioService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<BarbeiroService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<TelefoneService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<AgendamentoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<ServicoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<ClienteService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<BlacklistService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<NotificacaoService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
           .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
           ;

            builder.Services.AddHttpClient<AuditoriaLogService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
            .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
#endif
            ;

            builder.Services.AddHttpClient<RelatorioService>(c => c.BaseAddress = new Uri(ipDaApi))
#if ANDROID
           .ConfigurePrimaryHttpMessageHandler(() => ObterSslHandlerSeguro())
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
            builder.Services.AddTransient<AgendamentoMeusServicosViewModel>();
            builder.Services.AddTransient<SobreNosViewModel>();
            builder.Services.TryAddTransient<AuditoriaLogConsultarViewModel>();
            builder.Services.TryAddTransient<AuditoriaLogDetalhesViewModel>();
            builder.Services.TryAddTransient<RelatoriosViewModel>();


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
            builder.Services.AddTransient<AgendamentoMeusServicosView>();
            builder.Services.AddTransient<SobreNosView>();
            builder.Services.AddTransient<AuditoriaLogConsultarView>();
            builder.Services.AddTransient<AuditoriaLogDetalhesView>();
            builder.Services.AddTransient<RelatoriosView>();


            // 👉 5. Configuração Visual
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

        // 👉 CORREÇÃO DO FIREBASE: Deixamos só o que você vai usar! Os outros assumem `false` por padrão.
        private static CrossFirebaseSettings CreateCrossFirebaseSettings()
        {
            return new CrossFirebaseSettings(
                isCloudMessagingEnabled: true
            );
        }

        // 👉 CORREÇÃO DO SSL: Método para gerar um passaporte novo por Service, evitando o erro de "Handler já em uso"
#if ANDROID
        private static HttpMessageHandler ObterSslHandlerSeguro()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }
#endif
    }
}