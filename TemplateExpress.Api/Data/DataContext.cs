using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Entities;
namespace TemplateExpress.Api.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<TemplateEntity> Templates { get; set; } = null!;
    public DbSet<TemplateObjectEntity> TemplateObjects { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Username).IsRequired();
            entity.Property(u => u.ConfirmedAccount).IsRequired().HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).IsRequired().HasColumnType("timestamp without time zone");
            entity.Property(u => u.UpdatedAt).IsRequired().HasColumnType("timestamp without time zone");

        });

        modelBuilder.Entity<TemplateEntity>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.User)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.UserId);
            
            entity.Property(t => t.TemplateName).IsRequired();
            entity.Property(t => t.Width).IsRequired();
            entity.Property(t => t.Height).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired().HasColumnType("timestamp without time zone");
            entity.Property(u => u.UpdatedAt).IsRequired().HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<TemplateObjectEntity>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.Template)
                .WithMany(t => t.TemplateObjects)
                .HasForeignKey(t => t.TemplateId);
            
            entity.Property(t => t.FieldName).IsRequired();
            entity.Property(t => t.Italic).IsRequired();
            entity.Property(t => t.Bold).IsRequired();
            entity.Property(t => t.FontSize).IsRequired();
            entity.Property(t => t.FontFamily).IsRequired();
            entity.Property(t => t.FillStyle).IsRequired();
            entity.Property(t => t.XPoint).IsRequired();
            entity.Property(t => t.YPoint).IsRequired();
            entity.Property(t => t.BaseBoxHeight).IsRequired();
            entity.Property(t => t.FontBoundingBoxDescent).IsRequired();
            entity.Property(t => t.MaxRows).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired().HasColumnType("timestamp without time zone");
            entity.Property(u => u.UpdatedAt).IsRequired().HasColumnType("timestamp without time zone");
        });
    }
    
}
