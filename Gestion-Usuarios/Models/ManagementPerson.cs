using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_Usuarios.Models
{
    [Table("management_person_table")]
    public class ManagementPerson
    {
        [Key]
        public int management_person_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string management_person_FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string management_person_LastNamePaternal { get; set; } = string.Empty;

        [StringLength(100)]
        public string? management_person_LastNameMaternal { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string management_person_Email { get; set; } = string.Empty;

    }
}