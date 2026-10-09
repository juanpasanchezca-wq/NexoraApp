namespace NexoraApp.Views
{
    // Modelos de vista: datos ya "masticados" para enlazar con las tarjetas
    public record AvatarItem(string Iniciales, Color Fondo);

    public record ProyectoItem(
        int Id, string Nombre, string Descripcion, string Estado,
        Color FondoEstado, Color PuntoEstado,
        int Avance, double AvanceProgreso, string Entrega,
        List<AvatarItem> Avatares, string Extra, bool TieneExtra);

    public record TareaFila(int Id, string Titulo, string Estado, Color ColorEstado, string Vence, string Responsable)
    {
        public string Detalle => $"{Estado} · {Vence} · {Responsable}";
    }

    public record MiembroItem(string Iniciales, Color Fondo, string Nombre, string Rol);

    public record EstadoFila(string Estado, int Cantidad, double Progreso, Color Color);

    // Tarjeta de tarea (la usan la lista, el Kanban y las búsquedas)
    public record TareaCard(
        int Id, string Titulo, string Proyecto,
        string Prioridad, Color FondoPrioridad,
        string Estado, Color ColorEstado,
        string Vence, Color ColorVence,
        string Iniciales, Color FondoAvatar);

    // Un grupo = una columna del Kanban o una sección de la lista
    public record GrupoTareas(string Titulo, int Cantidad, Color Color, List<TareaCard> Tareas)
    {
        public bool Vacio => Cantidad == 0;
        public bool ConTareas => Cantidad > 0;
    }

    public record ComentarioItem(string Iniciales, Color Fondo, string Autor, string Texto, string Fecha);
}
