using CommunityToolkit.Mvvm.Messaging;
using MatutosDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosApp.Services
{
    public partial class UsuarioSessaoService
    {
        public static Usuario? UsuarioLogado { get; private set; }

        //public static event Action? OnSessaoChanged;

        public static void IniciarSessao(Usuario usuario)
        {
            UsuarioLogado = usuario;
            //OnSessaoChanged?.Invoke();
            WeakReferenceMessenger.Default.Send("SessaoAlterada");
        }

        public static void EncerrarSessao() // Método de Logout unificado
        {
            UsuarioLogado = null;
            //OnSessaoChanged?.Invoke(); // Dispara o evento de mudança
            WeakReferenceMessenger.Default.Send("SessaoAlterada");
        }
    }
}
