using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //_hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            //_hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            //_hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

            _gameServer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

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
