using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeepApp_API.Models
{
    public class User : IdentityUser
    {
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
