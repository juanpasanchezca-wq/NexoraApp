using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ruta del archivo SQLite del servidor (en Docker se puede montar un volumen en /data)
var ruta = builder.Configuration["RUTA_BD"] ?? "nexora-servidor.db";
builder.Services.AddDbContext<ServidorDb>(o => o.UseSqlite($"Data Source={ruta}"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ServidorDb>().Database.EnsureCreated();
}

// GET /api/Tarea  -> todas las tareas guardadas en el servidor
app.MapGet("/api/Tarea", async (ServidorDb db) =>
    await db.Tareas.OrderBy(t => t.Id)
        .Select(t => new TareaDto(t.Id, t.Titulo, t.Descripcion, t.Prioridad, t.Estado,
            t.FechaLimite, t.ProyectoId, t.UsuarioId, t.FechaCompletada))
        .ToListAsync());

// POST /api/Tarea -> recibe una lista de tareas; crea las nuevas y actualiza las existentes
app.MapPost("/api/Tarea", async (List<TareaDto> tareas, ServidorDb db) =>
{
    foreach (var dto in tareas)
    {
        var tarea = await db.Tareas.FindAsync(dto.Id);
        if (tarea == null)
        {
            tarea = new TareaServidor { Id = dto.Id };
            db.Tareas.Add(tarea);
        }
        tarea.Titulo = dto.Titulo;
        tarea.Descripcion = dto.Descripcion;
        tarea.Prioridad = dto.Prioridad;
        tarea.Estado = dto.Estado;
        tarea.FechaLimite = dto.FechaLimite;
        tarea.ProyectoId = dto.ProyectoId;
        tarea.UsuarioId = dto.UsuarioId;
        tarea.FechaCompletada = dto.FechaCompletada;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { recibidas = tareas.Count });
});

app.Run();
