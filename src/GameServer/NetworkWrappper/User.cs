using AutoMapper;
using GameServer.Common;
using GameServer.DAL;
using GameServer.Metagame;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace GameServer.NetworkWrappper
{
    public class User : IWithId<Guid>
    {
        private readonly IServiceProvider _serviceProvider;
        private bool _isActive = false;

        public NetworkClient Client { get; }
        public MetagameUser MetagameUser { get; set; }

        public Guid Id { get; }

        public User(Guid id, IServiceProvider serviceProvider)
        {
            Id = id;
            _serviceProvider = serviceProvider;
            Client = new NetworkClient(id);

            MetagameUser = new MetagameUser(Id,
                _serviceProvider.GetRequiredService<IUserRepository>(),
                _serviceProvider.GetRequiredService<IClientHolder>(),
                _serviceProvider.GetRequiredService<IMapper>());
        }

        public void ActiveMetagameUser()
        {
            Console.WriteLine("Metagame user with id " + Id + " created");
            _isActive = true;
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
