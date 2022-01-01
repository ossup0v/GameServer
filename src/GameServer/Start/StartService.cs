using Microsoft.Extensions.Hosting;

namespace GameServer.Start
{
    internal class StartService : IHostedService
    {
        private readonly IGameServer _gameServer;

        public StartService(IGameServer gameServer)
        {
            _gameServer = gameServer;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _gameServer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _gameServer.Stop();
            return Task.CompletedTask;
        }
        private void OnStarted()
        {
            // ...
        }

        private void OnStopping()
        {
            // ...
        }

        private void OnStopped()
        {
            // ...
        }
    }
}
