using Creators.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Creators.Extentions;

static class DbContextExtentions
{
    public static void SeedEnumValues<T, TEnum>(this DbSet<T> dbSet, Func<TEnum, T> converter)
        where T : class => Enum.GetValues(typeof(TEnum))
                               .Cast<object>()
                               .Select(value => converter((TEnum)value))
                               .ToList()
                               .ForEach(instance => dbSet.Update(instance));

    public static void SeedEnumValues<T, TEnum>(this EntityTypeBuilder<T> entityTypeBuilder, Func<TEnum, T> converter)
        where TEnum : Enum
        where T : class
    {
        var values = Enum.GetValues(typeof(TEnum))
                               .Cast<object>()
                               .Select(value => converter((TEnum)value));
        entityTypeBuilder.HasData(values);
    }
}