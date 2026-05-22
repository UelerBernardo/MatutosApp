using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatutosDomain
{
    public class AuthResponse
    {
        public string? Token { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
