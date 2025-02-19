using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NotebookDB.Database
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<NotebookAuthorizedUser> NotebookAuthorizedUsers { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<InstanceValue> Values { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<FileShard> FileShards { get; set; }
        
        public DbSet<TemplateNotebook> TemplateNotebooks { get; set; }
        public DbSet<TemplateSection> TemplateSections { get; set; }
        public DbSet<TemplateField> TemplateFields { get; set; }

        public DbSet<SystemSetting> SystemSettings { get ;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Field>()
            .HasOne(f => f.Section)
            .WithMany(s => s.Fields)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TemplateField>()
            .HasOne(f => f.Section)
            .WithMany(s => s.Fields)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<InstanceValue>()
            .HasOne(v => v.Instance)
            .WithMany(i => i.Values)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
