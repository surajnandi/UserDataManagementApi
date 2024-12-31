using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserdataManagement.Models
{
    public class UserDataModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("age")]
        public int Age { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("createdat", TypeName = "timestamp without time zone")]
        public DateTime? CreatedAt { get; set; }

        [Column("updatedat", TypeName = "timestamp without time zone")]
        public DateTime? UpdatedAt { get; set; }
    }
}
