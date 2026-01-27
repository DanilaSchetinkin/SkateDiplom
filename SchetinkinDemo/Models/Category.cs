using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CategoryPath> CategoryPathAncestors { get; set; } = new List<CategoryPath>();

    public virtual ICollection<CategoryPath> CategoryPathDescendants { get; set; } = new List<CategoryPath>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
