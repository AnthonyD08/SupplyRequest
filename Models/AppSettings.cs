namespace WebApi.Models
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string MinutosExpiracionToken { get; set; } = string.Empty;
        public string BaseDatosSeguridad { get; set; } = string.Empty;
    }
}