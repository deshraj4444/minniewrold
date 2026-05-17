using System.ComponentModel.DataAnnotations;
namespace MayaAstro.Models

{
    public class UserVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        public string Salt { get; set; }
        public int? RoleId { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class LoginVM
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Salt { get; set; }
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool EmailVerified { get; set; }
        public string AuthToken { get; set; }
        public int UserType { get; set; }
        public int Status { get; set; }
        public bool RememberMe { get; set; }



    }
    public class UserWebsiteVM
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int WebsiteId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
