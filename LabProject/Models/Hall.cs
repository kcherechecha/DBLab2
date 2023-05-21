using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabProject.Models;

public partial class Hall
{
    public int HallId { get; set; }
    [Required(ErrorMessage = "Назва зали обов'язкова")]
    [Display(Name ="Назва зали")]
    //[Remote("HallNameExists", "Halls", HttpMethod = "POST", ErrorMessage = "Зала з цією назвою вже існує в цьому кінотеатрі")]
    public string HallName { get; set; }
    [Required(ErrorMessage = "Міскість зали обов'язкова")]
    [Range(1, 900, ErrorMessage = "Така кількість місць неможлива (1-900)")]
    [Display(Name = "Кількість місць")]
    public int HallCapacity { get; set; }

    public int CinemaId { get; set; }

    [Display(Name = "Кінотеатр")]
    public virtual Cinema Cinema { get; set; }

    public virtual ICollection<Session> Sessions { get; } = new List<Session>();
}
