using System.ComponentModel.DataAnnotations;

namespace AllUp.Models;

public class Setting: BaseEntity
{
    public string Key { get; set; }

    [MaxLength(1000)]
    public string Value { get; set; }
}
