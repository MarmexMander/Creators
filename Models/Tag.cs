using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Creators.Models;
public class Tag
{
    [MaxLength(40), Key]
    public string Name{ get; set; }
    public List<Category> Categories{ get; set; }
    public Tag? AliasedTo{get; set;}
    public bool IsNSFW{ get; set; } = false;
}