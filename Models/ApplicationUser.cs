 using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CvManagement.Models;

public class ApplicationUser : IdentityUser
{
    // You can add custom properties here if needed
    public ICollection<CVSubmission> CVSubmissions { get; set; } = new List<CVSubmission>();
}