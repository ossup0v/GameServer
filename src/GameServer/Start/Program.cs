using GameServer.Common;
using GameServer.Configs;
using GameServer.DAL;
using GameServer.DAL.Mongo;
using GameServer.Metagame;
using GameServer.Metagame.GameRooms;
using GameServer.NetworkWrappper;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
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
                    .AddSingleton<GameServerConfig>(new GameServerConfig { ClientPort = 26954, GameRoomPort = 26949, MaxPlayerAmount = 150, MaxGameRoomAmount = 999 })
                    .AddSingleton<IGameServer, GameServer.NetworkWrappper.GameServer>()
                    .AddSingleton<IGameManager, GameManager>()
                    .AddSingleton<IServerSendToClient, ServerSendToClient>()
                    .AddSingleton<IServerSendToGameRoom, ServerSendToGameRoom>()
                    .AddSingleton<IClientHolder, ClientHolder>()
                    .AddSingleton<IGameRoomHolder, GameRoomHolder>()
                    .AddSingleton<IUserRepository, TempUserRepository>()
                    .AddSingleton<IGameRoomDataSender, GameRoomNetworkProcessor>()
                    .AddSingleton<IGameRoomDataReceiver, GameRoomNetworkProcessor>()
                    .AddSingleton<IClientDataSender, ClientNetworkProcessor>()
                    .AddSingleton<IClientDataReceiver, ClientNetworkProcessor>()
                    .AddHostedService<StartService>()
                    .AddHostedService<ServerClientPacketsHandler>()
                    .AddHostedService<ServerGameRoomPacketsHandler>()
                    .AddAutoMapper(config =>
                    {
                        config.AddProfile<AutoMapperProfile>();
                    })
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