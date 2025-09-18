using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PokemonContext>(options => options.UseSqlite("Data Source=pokemon.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PokemonContext>();
    db.Database.EnsureCreated();
    
    if (!db.Pokemon.Any())
    {
        var client = new HttpClient();
        var response = await client.GetFromJsonAsync<PokemonList>("https://pokeapi.co/api/v2/pokemon?limit=151");
        
        foreach (var item in response.Results)
        {
            var pokemon = await client.GetFromJsonAsync<PokemonData>(item.Url);
            db.Pokemon.Add(new Pokemon 
            { 
                Name = pokemon.Name, 
                Height = pokemon.Height, 
                Weight = pokemon.Weight 
            });
        }
        db.SaveChanges();
    }
}

app.MapGet("/pokemon", (PokemonContext db) => db.Pokemon.ToList());
app.MapGet("/pokemon/{name}", (PokemonContext db, string name) => 
    db.Pokemon.FirstOrDefault(p => p.Name.ToLower() == name.ToLower()));
app.Run();

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Height { get; set; }
    public int Weight { get; set; }
}

public class PokemonContext : DbContext
{
    public PokemonContext(DbContextOptions<PokemonContext> options) : base(options) { }
    public DbSet<Pokemon> Pokemon { get; set; }
}

public class PokemonList
{
    public List<PokemonItem> Results { get; set; } = new();
}

public class PokemonItem
{
    public string Url { get; set; } = "";
}

public class PokemonData
{
    public string Name { get; set; } = "";
    public int Height { get; set; }
    public int Weight { get; set; }
}
