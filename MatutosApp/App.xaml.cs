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
        }


        protected override void OnStart()
        {
            base.OnStart();

#if ANDROID
            CrossFirebaseCloudMessaging.Current.NotificationReceived += OnNotificacaoRecebida;
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

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