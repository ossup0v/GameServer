using Cysharp.Text;
using GameServer.Common;
using GameServer.Configs;
using GameServer.DAL.InMemory;
using GameServer.DAL.Interfaces;
using GameServer.Metagame;
using GameServer.Metagame.GameRooms;
using GameServer.NetworkWrappper;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;
using GameServer.Start;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var availablePorts = new List<int> { 26952, 26953, 26955, 26956, 26957, 26958, 26959, 26960 };
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddSingleton<IRoomManager, RoomManager>()
                    .AddSingleton<RoomManagerConfig>(new RoomManagerConfig { AvailablePorts = availablePorts })
                    .AddSingleton<GameServerConfig>(new GameServerConfig { ClientPort = 26954, GameRoomPort = 26949, MaxPlayerAmount = 150, MaxGameRoomAmount = availablePorts.Count })
                    .AddSingleton<IGameServer, GameServer.NetworkWrappper.GameServer>()
                    .AddSingleton<IGameManager, GameManager>()
                    .AddSingleton<IServerSendToClient, ServerSendToClient>()
                    .AddSingleton<IServerSendToGameRoom, ServerSendToGameRoom>()
                    .AddSingleton<IClientHolder, ClientHolder>()
                    .AddSingleton<IGameRoomHolder, GameRoomHolder>()
                    .AddSingleton<IUserRepository, InMemoryUserRepository>()
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
                    )
                    .ConfigureLogging(logging =>
                    {
                        // optional(MS.E.Logging):clear default providers.
                        logging.ClearProviders();

                        // optional(MS.E.Logging): default is Info, you can use this or AddFilter to filtering log.
                        logging.SetMinimumLevel(LogLevel.Debug);

                        // Add File Logging.
                        logging.AddZLoggerFile("fileName.log");

                        // Add Rolling File Logging.
                        logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);


                        logging.AddZLoggerConsole(options =>
                        {
#if DEBUG
                            // \u001b[31m => Red(ANSI Escape Code)
                            // \u001b[0m => Reset
                            // \u001b[38;5;***m => 256 Colors(08 is Gray)
                            var prefixFormat = ZString.PrepareUtf8<DateTime>("[{0}]");

                            options.PrefixFormatter = (writer, info) =>
                            {
                                if (info.LogLevel == LogLevel.Error)
                                {
                                    ZString.Utf8Format(writer, "\u001b[31m[{0}]", info.LogLevel);
                                }
                                else
                                {
                                    if (!info.CategoryName.StartsWith("GameServer")) // your application namespace.
                                    {
                                        ZString.Utf8Format(writer, "\u001b[38;5;08m[{0}]", info.LogLevel);
                                    }
                                    else
                                    {
                                        ZString.Utf8Format(writer, "[{0}]", info.LogLevel);
                                    }
                                }
                                prefixFormat.FormatTo(ref writer, info.Timestamp.DateTime.ToLocalTime());
                            };

                            options.SuffixFormatter = (writer, info) =>
                            {
                                if (info.LogLevel == LogLevel.Error || !info.CategoryName.StartsWith("GameServer"))
                                {
                                    ZString.Utf8Format(writer, "\u001b[0m", "");
                                }
                            };
#endif
                        }, configureEnableAnsiEscapeCode: true);

                    }).Build();


            await host.RunAsync();
        }
    }
}