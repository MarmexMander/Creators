using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Creators.Models;

public enum CategoryEnum{
    Art,
    Music,
    Prose,
    Poetry
}
public class Category{
    public Category(CategoryEnum categoryEnum)
    {
        Id = (int)categoryEnum;
        Name = categoryEnum.ToString();
    }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    
    [MaxLength(15)]
    public string Name { get; set; }

    public static implicit operator Category (CategoryEnum categoryEnum)
     => new Category (categoryEnum);
    public static implicit operator CategoryEnum (Category category)
     => (CategoryEnum) category.Id;
}       

