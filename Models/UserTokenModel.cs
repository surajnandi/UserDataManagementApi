namespace UserdataManagement.Models
{
    public class UserTokenModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string LoginToken { get; set; }
        public DateTime TokenExpiration { get; set; }
    }
}
