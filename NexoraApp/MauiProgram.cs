using Microsoft.Extensions.Logging;
using NexoraApp.DataAccess;
using NexoraApp.Views;

namespace NexoraApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Crea la base de datos en memoria y carga los datos iniciales (HasData)
            var dbContext = new NexoraDbContext();
            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

            // Rutas de las páginas de detalle (maestro - detalle)
            Routing.RegisterRoute(nameof(ProyectoDetailPage), typeof(ProyectoDetailPage));
            Routing.RegisterRoute(nameof(TareaDetailPage), typeof(TareaDetailPage));

            return builder.Build();
        }
    }
}
