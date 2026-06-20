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
            // 1. Limpa o usuário da memória RAM
            UsuarioLogado = null;

            // 👉 2. O DETALHE CRÍTICO: Apaga o token físico do armazenamento seguro do celular
            SecureStorage.Default.Remove("jwt_token");

            // 3. Dispara o evento para a interface (ex: esconder menus, mandar para tela de Login)
            WeakReferenceMessenger.Default.Send("SessaoAlterada");
        }
    }
}
