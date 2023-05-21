using System;
using System.Collections.Generic;

namespace LabProject.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; }

    public virtual ICollection<MovieCast> MovieCasts { get; } = new List<MovieCast>();
}
