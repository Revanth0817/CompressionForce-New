using System;
using System.Collections.Generic;

namespace CompressionForce.Domain.Entities;

public partial class UserSetting
{
    public int Id { get; set; }

    public DateTime? DateTime { get; set; }

    public int? PasswordExpiryDate { get; set; }

    public int? NoOfWrongAttempt { get; set; }

    public int? ApplicationLogout { get; set; }
}
