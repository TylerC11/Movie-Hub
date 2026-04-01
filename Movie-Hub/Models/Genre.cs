using System;
using System.Collections.Generic;

namespace Movie_Hub.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Title> Titles { get; set; } = new List<Title>();
}
