namespace Creators.Models;

[Flags]
enum Categorys{
    Music = 1 << 0,
    Photography = 1 << 1,
    Art = 1 << 2,
    Poetry = 1 << 3,
    Prose = 1 << 4,
    
}       
class Tag
{
    public string Name{ get; set; }
    public List<string> Aliases{ get; set; }
    public Categorys Categorys{ get; set; }
    public bool IsNSFW{ get; set; }
}