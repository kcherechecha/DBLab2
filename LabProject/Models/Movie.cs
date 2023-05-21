using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabProject.Models;

public partial class Movie
{
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Назва фільму обов'язкова")]
    [Display(Name = "Назва фільму")]
    public string MovieName { get; set; }

    [Required(ErrorMessage = "Тривалість фільму обов'язкова")]
    [Display(Name = "Тривалість (хв)")]
    [Range(15, 400, ErrorMessage ="Недопустима тривалість (15-400хв)")]
    public int MovieDuration { get; set; }

    [Display(Name = "Рейтинг")]
    [Range(1,5, ErrorMessage ="Недопустима оцінка (1-5)") ]
    public int? MovieRating { get; set; }

    [Required(ErrorMessage = "Дата виходу обов'язкова")]
    [Display(Name = "Дата виходу")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
    public DateTime MovieReleaseDate { get; set; }

    public virtual ICollection<MovieCast> MovieCasts { get; } = new List<MovieCast>();

    public virtual ICollection<MovieGenre> MovieGenres { get; } = new List<MovieGenre>();

    public virtual ICollection<Session> Sessions { get; } = new List<Session>();
}
