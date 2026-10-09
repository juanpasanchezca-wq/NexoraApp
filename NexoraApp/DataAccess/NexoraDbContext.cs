using Microsoft.EntityFrameworkCore;

namespace NexoraApp.DataAccess
{
    public class NexoraDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Proyecto> Proyectos { get; set; } = null!;
        public DbSet<Tarea> Tareas { get; set; } = null!;
        public DbSet<Miembro> Miembros { get; set; } = null!;
        public DbSet<Subtarea> Subtareas { get; set; } = null!;
        public DbSet<Comentario> Comentarios { get; set; } = null!;

        // Las fechas de las tareas se calculan desde hoy, así el resumen
        // (tareas de hoy, vencidas, completadas de la semana) siempre tiene datos.
        private static readonly DateTime Hoy = DateTime.Today;
        private static DateTime Dia(int dias) => Hoy.AddDays(dias);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("NexoraDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario(1, "Laura Méndez", "laura.mendez@nexora.app", "Administrador"),
                new Usuario(2, "Carlos Ruiz", "carlos.ruiz@nexora.app", "Líder de proyecto"),
                new Usuario(3, "Sofía Torres", "sofia.torres@nexora.app", "Miembro"),
                new Usuario(4, "Diego Fernández", "diego.fernandez@nexora.app", "Miembro"),
                new Usuario(5, "Valentina Gómez", "valentina.gomez@nexora.app", "Líder de proyecto"),
                new Usuario(6, "Andrés Castillo", "andres.castillo@nexora.app", "Miembro")
            );

            modelBuilder.Entity<Proyecto>().HasData(
                new Proyecto(1, "Rediseño del sitio web", "Renovación completa del sitio corporativo: nueva identidad visual, arquitectura de información y mejora del rendimiento en móviles.", new DateTime(2026, 8, 1), new DateTime(2026, 11, 15), "En curso", 2),
                new Proyecto(2, "App móvil de reservas", "Aplicación para que los clientes reserven citas, paguen en línea y reciban recordatorios.", new DateTime(2026, 8, 15), new DateTime(2026, 12, 5), "En curso", 5),
                new Proyecto(3, "Campaña de lanzamiento", "Plan de comunicación y contenidos para el lanzamiento del nuevo producto.", new DateTime(2026, 10, 20), new DateTime(2026, 12, 15), "Planificado", 5),
                new Proyecto(4, "Migración a la nube", "Traslado de servidores y bases de datos a la nube.", new DateTime(2026, 7, 1), new DateTime(2026, 10, 31), "Pausado", 6),
                new Proyecto(5, "Portal de clientes", "Portal de autogestión para clientes: facturas, tickets y documentos.", new DateTime(2026, 3, 1), new DateTime(2026, 9, 10), "Finalizado", 2)
            );

            modelBuilder.Entity<Miembro>().HasData(
                new Miembro(1, 1, 2), new Miembro(2, 1, 1), new Miembro(3, 1, 3), new Miembro(4, 1, 4),
                new Miembro(5, 2, 5), new Miembro(6, 2, 6), new Miembro(7, 2, 4),
                new Miembro(8, 3, 5), new Miembro(9, 3, 1),
                new Miembro(10, 4, 6), new Miembro(11, 4, 1),
                new Miembro(12, 5, 2), new Miembro(13, 5, 3)
            );

            modelBuilder.Entity<Subtarea>().HasData(
                new Subtarea(1, 7, "Wireframe de la página móvil", true),
                new Subtarea(2, 7, "Versión de escritorio", false),
                new Subtarea(3, 7, "Preparar prototipo para la revisión", false),
                new Subtarea(4, 8, "Credenciales del entorno de pruebas", true),
                new Subtarea(5, 8, "Flujo de pago completo", false),
                new Subtarea(6, 1, "Texto de misión", true),
                new Subtarea(7, 1, "Texto de visión", false),
                new Subtarea(8, 1, "Perfiles del equipo", false)
            );

            modelBuilder.Entity<Comentario>().HasData(
                new Comentario(1, 7, 1, "Recuerden usar la nueva paleta de la guía de marca.", Hoy.AddDays(-1).AddHours(10)),
                new Comentario(2, 7, 3, "Listo el wireframe móvil, sigo con la versión de escritorio.", Hoy.AddHours(9)),
                new Comentario(3, 8, 6, "La pasarela responde bien en el entorno de pruebas.", Hoy.AddDays(-1).AddHours(15))
            );

            modelBuilder.Entity<Tarea>().HasData(
                new Tarea(1, "Redactar textos de la página \"Nosotros\"", "Escribir los textos de misión, visión y equipo.", "Media", "Por hacer", Dia(3), 1, 3),
                new Tarea(2, "Revisar presupuesto del proyecto", "Validar el presupuesto con finanzas antes de la siguiente fase.", "Alta", "Por hacer", Dia(-1), 1, 1),
                new Tarea(3, "Pantalla de confirmación de reserva", "Diseñar la pantalla que se muestra al confirmar una reserva.", "Media", "Por hacer", Dia(0), 2, 4),
                new Tarea(4, "Pruebas de usabilidad", "Probar el flujo de reserva con cinco usuarios.", "Alta", "Por hacer", Dia(10), 2, 4),
                new Tarea(5, "Calendario de publicaciones", "Definir las fechas y canales de cada publicación.", "Baja", "Por hacer", Dia(20), 3, 5),
                new Tarea(6, "Plan de respaldo de datos", "Documentar copias de seguridad y recuperación.", "Alta", "Por hacer", Dia(-4), 4, 1),
                new Tarea(7, "Diseñar maqueta de inicio", "Crear la maqueta de alta fidelidad de la página de inicio siguiendo la nueva guía de marca. Incluir versión móvil y de escritorio, y preparar el prototipo para la revisión con el cliente.", "Alta", "En progreso", Dia(0), 1, 3),
                new Tarea(8, "Integrar pasarela de pago", "Conectar la pasarela de pago con el flujo de reservas.", "Alta", "En progreso", Dia(0), 2, 6),
                new Tarea(9, "Definir público objetivo", "Describir los segmentos de clientes de la campaña.", "Media", "En revisión", Dia(0), 3, 5),
                new Tarea(10, "Guía de estilos de marca", "Colores, tipografías y componentes de la marca.", "Media", "En revisión", Dia(2), 1, 1),
                new Tarea(11, "Configurar analítica web", "Instalar y validar la analítica en el sitio.", "Media", "Hecha", Dia(-2), 1, 3) { FechaCompletada = Dia(-1) },
                new Tarea(12, "Mapa del sitio", "Definir la estructura de páginas del nuevo sitio.", "Baja", "Hecha", Dia(-10), 1, 4) { FechaCompletada = Dia(-2) },
                new Tarea(13, "Migrar base de datos", "Trasladar la base de datos de producción a la nube.", "Alta", "Hecha", Dia(-20), 4, 6) { FechaCompletada = Dia(-2) }
            );
        }
    }

    public record Usuario(int Id, string Nombre, string Correo, string Rol);

    public record Proyecto(int Id, string Nombre, string Descripcion, DateTime FechaInicio, DateTime FechaFin, string Estado, int ResponsableId)
    {
        public Usuario Responsable { get; set; } = null!;
    }

    public record Tarea(int Id, string Titulo, string Descripcion, string Prioridad, string Estado, DateTime FechaLimite, int ProyectoId, int UsuarioId)
    {
        public DateTime? FechaCompletada { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }

    // Miembros de cada proyecto (relación muchos a muchos entre Proyecto y Usuario)
    public record Miembro(int Id, int ProyectoId, int UsuarioId)
    {
        public Proyecto Proyecto { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }

    public record Subtarea(int Id, int TareaId, string Titulo, bool Completada);

    public record Comentario(int Id, int TareaId, int UsuarioId, string Texto, DateTime Fecha)
    {
        public Usuario Usuario { get; set; } = null!;
    }
}
