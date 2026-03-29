using System.ComponentModel.DataAnnotations;

namespace CvManagement.Models;

public class CVSubmission
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Position Applying For")]
    public string Position { get; set; } = string.Empty;

    [Display(Name = "Additional Information")]
    public string AdditionalInfo { get; set; } = string.Empty;

    [Display(Name = "CV File URL")]
    public string CVFileUrl { get; set; } = string.Empty;

    [Display(Name = "CV File Name")]
    public string CVFileName { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!; // Navigation property; will be set by EF Core
}