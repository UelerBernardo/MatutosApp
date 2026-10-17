using MatutosDomain;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
[Table("auditoria_log")]
public class AuditoriaLog
{
    public long Codigo_Log { get; set; }
    public string? Entidade_Afetada { get; set; }
    public string? Registro_ID { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoAcaoEnum Tipo_Acao { get; set; }
    public string? Valores_Antigos { get; set; }
    public string? Valores_Novos { get; set; }
    public int? Usuario_Acao { get; set; }
    public DateTime Data_Hora_Acao { get; set; }
}