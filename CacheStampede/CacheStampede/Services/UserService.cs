using System;
using CacheStampede.Model;
using Microsoft.Extensions.Primitives;

namespace CacheStampede.Services;

public static class UserService
{
    private static List<User> Users { get; set; } = [];
    static UserService()
    {
        Users = [
            new()
            {
                UserID = 1,
                UserName = "Praveen",
                Age = 24,
                Role = "Software Enginner"
            },
            new()
            {
                UserID = 2,
                UserName = "Kumar",
                Age = 25,
                Role = "Senior Software Engineer"
            }
        ];
    }

    public static User? GetUser(int userID)
    {
        return Users.FirstOrDefault(x => x.UserID==userID);
    }
}
