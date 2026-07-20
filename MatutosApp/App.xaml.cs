using Microsoft.Maui; // 👉 Resolve o IActivationState
using Microsoft.Maui.Controls; // 👉 Resolve o Application
using MatutosApp.Views;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Microsoft.Maui.ApplicationModel; // 👉 Resolve o MainThread
using System.Threading.Tasks;
namespace MatutosApp
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
            // ❌ A linha do Firebase NÃO fica mais aqui!
        }

        // 👉 O radar entra aqui! Esse método roda com segurança logo após o app abrir.
        protected override void OnStart()
        {
            base.OnStart();
            // 👉 O C# só vai tentar ligar o Firebase se estiver rodando em um celular Android
#if ANDROID
            CrossFirebaseCloudMessaging.Current.NotificationReceived += OnNotificacaoRecebida;
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        //private void OnNotificacaoRecebida(object sender, FCMNotificationReceivedEventArgs e)
        //{
        //    var titulo = e.Notification.Title;
        //    var mensagem = e.Notification.Body;

        //    // Envia a ordem para a Thread principal
        //    MainThread.BeginInvokeOnMainThread(async () =>
        //    {
        //        // 1. TRAVA DE SEGURANÇA: O código fica "patinando" aqui até a tela principal ser criada
        //        while (Application.Current?.MainPage == null)
        //        {
        //            await Task.Delay(500); // Espera meio segundo e tenta de novo
        //        }

        //        // 2. GORDURA DE TEMPO: Espera mais 1 segundo para garantir que a animação 
        //        // da tela de carregamento (Splash Screen) já acabou e o usuário está vendo o app
        //        await Task.Delay(1000);

        //        // 3. Agora sim, desenha a notificação com a tela pronta!
        //        var snackbarOptions = new SnackbarOptions
        //        {
        //            BackgroundColor = Colors.Black,
        //            TextColor = Colors.White,
        //            CornerRadius = new CornerRadius(10)
        //        };

        //        var snackbar = Snackbar.Make($"🔔 {titulo}: {mensagem}", null, "OK", TimeSpan.FromSeconds(8), snackbarOptions);
        //        await snackbar.Show();
        //    });
        //}
#if ANDROID
        private void OnNotificacaoRecebida(object sender, Plugin.Firebase.CloudMessaging.EventArgs.FCMNotificationReceivedEventArgs e)
        {
            var titulo = e.Notification.Title;
            var mensagem = e.Notification.Body;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Trava de segurança para esperar a tela principal carregar
                while (Application.Current?.MainPage == null)
                {
                    await Task.Delay(500);
                }
                await Task.Delay(1000);

                await Application.Current.MainPage.DisplayAlert($"🔔 {titulo}", mensagem, "Entendi");
            });
        }
#endif
    }
}