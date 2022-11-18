using CatalogoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Context;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  { }

  public DbSet<Categoria> Categorias { get; set; }
  public DbSet<Produto> Produtos { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Categoria>().HasKey(c => c.CategoriaId);
    modelBuilder.Entity<Categoria>().Property(c => c.Nome).IsRequired().HasMaxLength(100);
    modelBuilder.Entity<Categoria>().Property(c => c.Descricao).IsRequired().HasMaxLength(255);

    modelBuilder.Entity<Produto>().HasKey(p => p.ProdutoId);
    modelBuilder.Entity<Produto>().Property(p => p.Nome).IsRequired().HasMaxLength(100);
    modelBuilder.Entity<Produto>().Property(p => p.Descricao).IsRequired().HasMaxLength(255);
    modelBuilder.Entity<Produto>().Property(p => p.Preco).IsRequired().HasPrecision(14, 2);
    modelBuilder.Entity<Produto>().Property(p => p.Imagem).IsRequired().HasMaxLength(255);

    modelBuilder.Entity<Produto>()
      .HasOne(p => p.Categoria)
      .WithMany(c => c.Produtos)
      .HasForeignKey(p => p.CategoriaId);
  }
}