using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatutosDomain
{
    [Table("Usuario")] 
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Codigo_Usuario { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(250)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [MaxLength(250)]
        [Column("E_mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MaxLength(250)]
        public string Senha { get; set; } = string.Empty;

        [NotMapped]
        public UsuarioTipo TipoSelecionado { get; set; }
        public bool Ativo { get; set; } = true;
        public string? Codigo_Recuperacao { get; set; }
        public DateTime? Data_Validade_Codigo { get; set; }
        public string? TokenFCM { get; set; }
        public string? Imagem_Usuario { get; set; }

        [JsonIgnore]
        public virtual ICollection<UsuarioTelefone> UsuariosTelefones { get; set; } = new List<UsuarioTelefone>();

        public string UrlImagemCompleta
        {
            get
            {
                if (string.IsNullOrEmpty(Imagem_Usuario))
                    return "icon_user_white.png"; // Retorna o ícone padrão se não tiver foto

                // Troque pela URL base da sua API
                string urlBase = "https://localhost:7110/";

                // Junta a base com o caminho que veio do banco
                return $"{urlBase}{Imagem_Usuario}";
            }
        }

    }
}
