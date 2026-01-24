using System;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CacheStampede.Model;

public class User
{
    public int UserID { get; set; }
    public string? UserName { get; set; }
    public int Age { get; set; }
    public string? Role { get; set; }
}
