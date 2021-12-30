
using GameServer.Configs;
using GameServer.DAL;
using GameServer.DAL.Mongo;
using GameServer.Metagame;
using GameServer.Metagame.GameRoom;
using GameServer.Network;
using GameServer.Start;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddSingleton<IRoomManager, RoomManager>()
                    .AddSingleton<RoomManagerConfig>(new RoomManagerConfig { AvailablePorts = new List<int> { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 } })
                    .AddSingleton<GameServerConfig>(new GameServerConfig { Port = 26954, MaxPlayers = 150 })
                    .AddSingleton<IGameServer, GameServer.Network.GameServer>()
                    .AddSingleton<IGameManager, GameManager>()
                    .AddSingleton<IServerSend, ServerSend>()
                    .AddSingleton<IServerHandler, ServerHandler>()
                    .AddSingleton<IClientHolder, ClientHolder>()
                    .AddSingleton<IUserRepository, TempUserRepository>()
                    .AddSingleton<IDataSender, NetworkProcessor>()
                    .AddSingleton<IDataReceiver, NetworkProcessor>()
                    .AddHostedService<StartService>()
                    .AddHostedService<ServerHandler>()
                //.Configure<RoomManagerConfig>(x =>
                //{
                //    x.AvailablePorts = new List<int> { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 };
                //})
                //.Configure<GameServerConfig>(x =>
                //{
                //    x.Port = 26954;
                //    x.MaxPlayers = 150;
                //})
                ).Build();


            await host.RunAsync();
        }
    }
}