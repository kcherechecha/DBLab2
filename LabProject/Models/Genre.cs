using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabProject.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    [Display(Name = "Назва жанру")]
    public string GenreName { get; set; }

    public virtual ICollection<MovieGenre> MovieGenres { get; } = new List<MovieGenre>();
}
