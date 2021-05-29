using System;
using System.Linq;
using SourceGeneratorExample.Database;

namespace SourceGeneratorExample.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new DbContext();

            database.Add(new User { Id = 1, Name = "user 1" });
            database.Add(new User { Id = 2, Name = "user 2" });

            database.Add(new Blog { Id = 1, Name = "blog 1" });

            Console.WriteLine($"Database contains {database.GetAllUsers().Count()} users");
            Console.WriteLine($"Database contains {database.GetAllBlogs().Count()} blogs");

            var user = database.GetUser(1);

            Console.WriteLine($"User with id {user.Id} found. Name: {user.Name}");

            database.Remove(user);

            Console.WriteLine($"After removing database contains {database.GetAllUsers().Count()} users");
        }
    }
}
