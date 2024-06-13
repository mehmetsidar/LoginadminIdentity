using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
    public class EditViewModel
    {
        
        public string? Id { get; set; } 
     
        public string? UserName { get; set; } 

        public string? FulName { get; set; } 
        
        [EmailAddress]
        public string? Email { get; set; } 

        [Phone]
        public string? PhoneNumber { get; set; } 

        [DataType(DataType.Password)]
        public string? Password { get; set; } 

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parola Eşleşmiyor..")]

        public string? ConfirmPassword { get; set; } 

        public  IList<string>? SelectedRoles { get; set; }



    }
}