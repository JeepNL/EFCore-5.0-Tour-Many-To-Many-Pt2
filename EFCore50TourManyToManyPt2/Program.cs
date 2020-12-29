using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
	static void Main(string[] args)
	{
		using (var context = new GroupsContext())
		{
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			var maurycy = new User { Name = "Maurycy" };
			var arthur = new User { Name = "Arthur" };
			var andriy = new User { Name = "Andriy" };
			var brice = new User { Name = "Brice" };
			var smit = new User { Name = "Smit" };
			var shay = new User { Name = "Shay" };
			var jeremy = new User { Name = "Jeremy" };

			var efTeam = new Group { Name = "EF Team" };
			var queryTeam = new Group { Name = "Query Team" };
			var managers = new Group { Name = "Managers" };
			var engineers = new Group { Name = "Engineers" };

			efTeam.Users.AddRange(new[] { maurycy, arthur, andriy, brice, smit, shay, jeremy });
			queryTeam.Users.AddRange(new[] { maurycy, smit });
			managers.Users.AddRange(new[] { arthur, jeremy });
			engineers.Users.AddRange(new[] { maurycy, andriy, brice, smit, shay });

			context.AddRange(maurycy, arthur, andriy, brice, smit, shay, jeremy, efTeam, queryTeam, managers, engineers);

			context.SaveChanges();
		}

		using (var context = new GroupsContext())
		{
			var users = context.Users.Include(e => e.Groups).ToList();

			Console.WriteLine();
			Console.WriteLine("ON.NET SHOW: EF Core 5.x Tour Many To Many Part 2");
			Console.WriteLine();
			Console.WriteLine("Users with Groups");
			Console.WriteLine();

			foreach (var user in users)
			{
				Console.WriteLine($"User: {user.Name}");
				foreach (var group in user.Groups)
				{
					Console.WriteLine($"  Group: {group.Name}");
				}
			}
		}

	}
}

public class User
{
	public int Id { get; set; }
	public string Name { get; set; }

	public List<Group> Groups { get; } = new List<Group>();
}

public class Group
{
	public int Id { get; set; }
	public string Name { get; set; }

	public List<User> Users { get; } = new List<User>();
}

public class GroupsContext : DbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Group> Groups { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder
		.LogTo(Console.WriteLine, LogLevel.Information)
		.EnableSensitiveDataLogging()
		.UseSqlite("Data Source = EFCoreTour.db");

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// modelBuilder.SharedTypeEntity<Dictionary<string, object>>("GroupUser"); // Commented because code below defines this

		modelBuilder
			.Entity<User>()
			.HasMany(e => e.Groups)
			.WithMany(e => e.Users)
			.UsingEntity<Dictionary<string, object>>(
				"Memberships", // (Join) Table Name. Default = GroupUser
				b => b.HasOne<Group>().WithMany().HasForeignKey("GroupId"), // Field Name. Default = GroupsId (Groups, with an 's')
				b => b.HasOne<User>().WithMany().HasForeignKey("UserId") // Field Name. Default = UsersId (Ditto)
			);
	}
}

