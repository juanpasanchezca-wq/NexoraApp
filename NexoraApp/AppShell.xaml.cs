using Microsoft.Maui.ApplicationModel;

namespace NexoraApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            var uri = new Uri("https://learn.microsoft.com/dotnet/maui/");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
    }
}
