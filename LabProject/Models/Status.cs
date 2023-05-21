using System;
using System.Collections.Generic;

namespace LabProject.Models;

public partial class Status
{
    public int StatusId { get; set; }

    public string StatusName { get; set; }

    public virtual ICollection<Session> Sessions { get; } = new List<Session>();
}
