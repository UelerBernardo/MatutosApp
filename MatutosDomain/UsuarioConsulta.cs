using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public  class UsuarioConsulta
    {
        public int Codigo_Usuario { get; set; }
        public string? Nome { get; set; }
        public UsuarioTipo TipoSelecionado { get; set; }
        public string? Email { get; set; }
        public bool Ativo { get; set; }
        public string? Imagem_Usuario { get; set; }
        public string? DDD { get; set; }
        public string? Numero_Telefone { get; set; }

        public string UrlImagemCompleta
        {
            get
            {
                if (string.IsNullOrEmpty(Imagem_Usuario))
                    return "usuariosistema.png";
                string urlBase = "https://localhost:7110/";

                return $"{urlBase}{Imagem_Usuario}";
            }
        }
    }
}
