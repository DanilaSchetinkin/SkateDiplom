using System;
using System.Collections.Generic;

namespace SchetinkinDemo.Models;

/// <summary>
/// Таблица замыкания для хранения иерархии категорий. Хранит все пути от предков к потомкам.
/// </summary>
public partial class CategoryPath
{
    public int AncestorId { get; set; }

    public int DescendantId { get; set; }

    public int Depth { get; set; }

    public virtual Category Ancestor { get; set; } = null!;

    public virtual Category Descendant { get; set; } = null!;
}
