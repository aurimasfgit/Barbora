namespace Barbora.Core.Models
{
    public class AppSettings
    {
        public Credentials Credentials { get; set; }
    }

    public class Credentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}