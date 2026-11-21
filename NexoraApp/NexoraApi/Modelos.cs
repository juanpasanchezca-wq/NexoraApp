using Microsoft.EntityFrameworkCore;

// Lo que viaja por la red (lo mismo que envía y recibe la app MAUI)
public record TareaDto(
    int Id, string Titulo, string Descripcion, string Prioridad, string Estado,
    DateTime FechaLimite, int ProyectoId, int UsuarioId, DateTime? FechaCompletada);

// Lo que se guarda en la base del servidor
public class TareaServidor
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public string Estado { get; set; } = "";
    public DateTime FechaLimite { get; set; }
    public int ProyectoId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime? FechaCompletada { get; set; }
}

public class ServidorDb : DbContext
{
    public ServidorDb(DbContextOptions<ServidorDb> opciones) : base(opciones) { }

    public DbSet<TareaServidor> Tareas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // El Id lo define la app, no la base del servidor
        modelBuilder.Entity<TareaServidor>().Property(t => t.Id).ValueGeneratedNever();
    }
}
