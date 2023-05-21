using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabProject.Models;

public partial class MovieCast
{
    public int MovieCastId { get; set; }

    [Display(Name = "Ім'я")]
    public int CastMemberId { get; set; }

    [Display(Name = "Позиція")]
    public int PositionId { get; set; }

    public int MovieId { get; set; }

    //[Required(ErrorMessage = "Обов'язково оберіть людину")]
    [Display(Name = "Ім'я")]
    public virtual CastMember CastMember { get; set; }

    //[Required(ErrorMessage = "Обов'язково вкажіть фільм")]
    [Display(Name = "Фільм")]
    public virtual Movie Movie { get; set; }

    //[Required(ErrorMessage = "Позиція обов'язкова")]
    [Display(Name = "Позиція")]
    public virtual Position Position { get; set; }
}