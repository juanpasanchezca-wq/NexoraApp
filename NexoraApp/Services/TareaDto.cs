namespace NexoraApp.Services
{
    // Mismo contrato que usa el backend: lo que se envía y recibe por la red
    public record TareaDto(
        int Id, string Titulo, string Descripcion, string Prioridad, string Estado,
        DateTime FechaLimite, int ProyectoId, int UsuarioId, DateTime? FechaCompletada);
}
