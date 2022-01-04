using AutoMapper;
using GameServer.Common;
using GameServer.DAL;
using GameServer.DAL.Interfaces;
using GameServer.Metagame;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using ZLogger;

namespace GameServer.NetworkWrappper
{
    public class User : IWithId<Guid>
    {
        private readonly ILogger<User> _log;
        private readonly IServiceProvider _serviceProvider;


        public User(Guid id, ILogger<User> log, IServiceProvider serviceProvider)
        {
            Id = id;
            _log = log;
            _serviceProvider = serviceProvider;
            Client = new NetworkClient(id);

            MetagameUser = new MetagameUser(Id,
                _serviceProvider.GetRequiredService<IUserRepository>(),
                _serviceProvider.GetRequiredService<IClientHolder>(),
                _serviceProvider.GetRequiredService<IMapper>());
        }
        
        public NetworkClient Client { get; }
        public MetagameUser MetagameUser { get; set; }
        public Guid Id { get; }

        public void JoinToServer()
        {
            _log.ZLogInformation("Metagame user with id " + Id + " join to server!");
        }

        public void Connect(TcpClient tcpClient)
        {
            Client.tcp.Connect(tcpClient, Client);
        }

        public void SubsctibeToReceivePackets(Action<Guid, int, Packet> callback)
        {
            Client.tcp.PacketReceived += callback;
            Client.udp.PacketReceived += callback;
        }
    }
}
