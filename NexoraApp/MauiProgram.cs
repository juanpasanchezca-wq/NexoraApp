using Microsoft.Extensions.Logging;
using NexoraApp.DataAccess;
using NexoraApp.Services;
using NexoraApp.ViewModels;
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

            // Contenedor de dependencias (Clase 11)
            //  Singleton: una sola instancia para toda la app (servicios)
            //  Transient: instancia nueva cada vez que se pide (vistas y ViewModels)
            builder.Services.AddSingleton<INavegacionService, NavegacionService>();
            builder.Services.AddTransient<TareaDetailViewModel>();
            builder.Services.AddTransient<TareaDetailPage>();

            // Backend y conectividad (Clase 13)
            builder.Services.AddSingleton<ISincronizacionService, SincronizacionService>();
            builder.Services.AddTransient<SincronizacionViewModel>();
            builder.Services.AddTransient<SincronizacionPage>();

            // Crea el archivo SQLite la primera vez y carga los datos iniciales (HasData).
            // Si el archivo ya existe, se conservan los datos guardados.
            var dbContext = new NexoraDbContext();
            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

            // Rutas de las páginas de detalle (maestro - detalle)
            Routing.RegisterRoute(nameof(ProyectoDetailPage), typeof(ProyectoDetailPage));
            Routing.RegisterRoute(nameof(TareaDetailPage), typeof(TareaDetailPage));
            Routing.RegisterRoute(nameof(ProyectoFormPage), typeof(ProyectoFormPage));
            Routing.RegisterRoute(nameof(TareaFormPage), typeof(TareaFormPage));
            Routing.RegisterRoute(nameof(NotificacionesPage), typeof(NotificacionesPage));

            return builder.Build();
        }
    }
}
