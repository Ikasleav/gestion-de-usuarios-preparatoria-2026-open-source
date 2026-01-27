using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_Usuarios.Models
{
	[Table("management_user_table")]
	public class ManagementUser
	{
		[Key]
		public int management_user_ID { get; set; }

		public int? management_user_PersonID { get; set; }

		[Required]
		public string management_user_Username { get; set; }

		public string? management_user_Email { get; set; }

		[Required]
		public string management_user_PasswordHash { get; set; }

		public bool management_user_IsLocked { get; set; }

		public string? management_user_LockReason { get; set; }

		public DateTime? management_user_LastLoginDate { get; set; }

		public bool management_user_status { get; set; }

		public DateTime management_user_createdDate { get; set; }
	}
}