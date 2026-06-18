using System.Threading;
namespace Capa_Usuario
{
    public class HostedService
    {
        private Timer _timer;
        public void StartAsync()
        {
            //Metodos para ejecutarse en forma asincrona
            //Timer = new Timer(migracion, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }
        public void StopAsync()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }
        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}