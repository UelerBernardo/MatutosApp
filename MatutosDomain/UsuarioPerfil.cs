using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class UsuarioPerfil
    {
        public int Codigo_Usuario { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public bool Ativo { get; set; }
        public string? Imagem_Usuario { get; set; }
        public  int? NotificacaoNaoLidas { get; set; }
    } 
}
