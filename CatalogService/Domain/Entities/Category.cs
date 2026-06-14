namespace CatalogService.Domain.Entities;

public sealed class Category
{
  public Guid Id { get; private set; }
  public string Name { get; private set; }

  public ICollection<Product> Products { get; private set; }

  private Category() { } // EF Core

  private Category(string name)
  {
    Id = Guid.NewGuid();
    Name = name;
    Products = new List<Product>();
  }

  public static Category Create(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Category name is required.", nameof(name));

    return new Category(name);
  }

  public void Update(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Category name is required.", nameof(name));

    Name = name;
  }
}