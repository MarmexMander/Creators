using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Creators.Models;

[Flags]
public enum Categorys{
    Music = 1 << 0,
    Photography = 1 << 1,
    Art = 1 << 2,
    Poetry = 1 << 3,
    Prose = 1 << 4,
    
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