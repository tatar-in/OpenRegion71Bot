using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;

namespace OpenRegion71Bot
{
    class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext()
        {
            if (!File.Exists(ConfidentialData.BotDataBase))
            {
                Database.EnsureCreated();
                Users.Add(new DbData.User() { Id = 121231592, Name = "", Nick = "", IsBot = false, Rules = new List<DbData.Rule>() { Rules.Find(1) } });
                SaveChanges();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source = " + ConfidentialData.BotDataBase);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbData.Problem>().ToTable("Problems");
            modelBuilder.Entity<DbData.Theme>().ToTable("Themes");
            modelBuilder.Entity<DbData.Category>().ToTable("Categories");
            modelBuilder.Entity<DbData.Status>().ToTable("Statuses").HasData(new DbData.Status[]
            {
                new DbData.Status() { Id = 289, Name = "на рассмотрении модератора" },
                new DbData.Status() { Id = 290, Name = "принято модератором, рассматривается исполнителем" },
                new DbData.Status() { Id = 291, Name = "обращение отклонено" },
                new DbData.Status() { Id = 292, Name = "принято исполнителем, ожидается ответ" },
                new DbData.Status() { Id = 293, Name = "дан промежуточный ответ" },
                new DbData.Status() { Id = 294, Name = "получен ответ от исполнителя, ожидается принятие модератором" },
                new DbData.Status() { Id = 295, Name = "ответ дан и принят модератором" },
                new DbData.Status() { Id = 305, Name = "на утверждении" },
                new DbData.Status() { Id = 440, Name = "направлено на регистрацию в электронную приемную" },
                new DbData.Status() { Id = 1179, Name = "поставлено на контроль" },
            });
            modelBuilder.Entity<DbData.District>().ToTable("Districts");
            modelBuilder.Entity<DbData.Executor>().ToTable("Executors");
            modelBuilder.Entity<DbData.Source>().ToTable("Sources");
            modelBuilder.Entity<DbData.SourceCategory>().ToTable("SourceCategories");
            modelBuilder.Entity<DbData.Rule>().ToTable("Rules").HasData(new DbData.Rule[] 
            {
                new DbData.Rule() { Id = 1, Name = "Администратор" },
                new DbData.Rule() { Id = 2, Name = "Изменение обращений" },
                new DbData.Rule() { Id = 100, Name = "Без прав" },
            });
            modelBuilder.Entity<DbData.User>().ToTable("Users");
        }
        public DbSet<DbData.Problem> Problems { get; set; }
        public DbSet<DbData.Theme> Themes { get; set; }
        public DbSet<DbData.Category> Categories { get; set; }
        public DbSet<DbData.Status> Statuses { get; set; }
        public DbSet<DbData.District> Districts { get; set; }
        public DbSet<DbData.Executor> Executors { get; set; }
        public DbSet<DbData.Source> Sources { get; set; }
        public DbSet<DbData.SourceCategory> SourceCategories { get; set; }
        public DbSet<DbData.Rule> Rules { get; set; }
        public DbSet<DbData.User> Users { get; set; }
    }
}
