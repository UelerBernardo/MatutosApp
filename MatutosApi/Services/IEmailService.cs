namespace MatutosApi.Services
{
    public interface IEmailService
    {
        Task EnviarEmailRecuperacaoAsync(string emailDestino, string codigo);
    }
}
