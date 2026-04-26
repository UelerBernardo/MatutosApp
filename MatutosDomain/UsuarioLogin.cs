using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public  class UsuarioLogin
    {
        public string? Email { get; set; }
        public string? Senha { get; set; }
    }
}
