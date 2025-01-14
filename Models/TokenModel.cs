using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserdataManagement.Models
{
    public class TokenModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("token")]
        public string? Token { get; set; }

        [Column("expiration_time", TypeName = "timestamp without time zone")]
        public DateTime? ExpirationTime { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("created_at", TypeName = "timestamp without time zone")]
        public DateTime? CreatedAt { get; set; }
    }
}
