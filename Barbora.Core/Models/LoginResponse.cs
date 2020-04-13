namespace Barbora.Core.Models
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public bool rememberMe { get; set; }
        public string user_id { get; set; }
        public Message messages { get; set; }
    }
}