namespace Creators.Models;

class TagInfo
{
    public string Id { get; set; }
    public Tag Tag { get; set; }
    public virtual List<string> Aliases{ get; set; }
    //TODO: localizable aliases?
    //TODO: Lazy load. Maybe manually create middleman model pos-tag and make tag model virtual to built-in lazyload
    public virtual Categorys Categorys{ get; set; }
    public virtual bool IsNSFW{ get; set; }
}