using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Creators.Models;

public class Category{
    public int Id { get; set; }
    public string Name { get; set; }
}       

public class Tag
{
    public Tag()
    {
    }
    private Tag(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    ILazyLoader _lazyLoader;
    TagInfo _info;

    [Key]
    public string Name{ get; set; }
    public TagInfo Info{ 
        get => _lazyLoader.Load(this, ref _info);
        set => _info = value; }
    
}