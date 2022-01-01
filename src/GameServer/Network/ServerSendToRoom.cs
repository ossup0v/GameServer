using GameServer.Network.Holders;

namespace GameServer.Network
{
    public interface IServerSendToGameRoom 
    {
        void GameRoomData(Guid toGameRoom);
    }

    public class ServerSendToGameRoom : IServerSendToGameRoom
    {
        private readonly IGameRoomHolder _gameRoomHolder;
        private readonly IGameRoomDataSender _dataSender;
        private readonly IServiceProvider _serviceProvider;

        public ServerSendToGameRoom(IGameRoomHolder gameRoomHolder, 
            IGameRoomDataSender dataSender, 
            IServiceProvider serviceProvider)
        {
            _gameRoomHolder = gameRoomHolder;
            _dataSender = dataSender;
            _serviceProvider = serviceProvider;
        }

        #region SendBase
        private void SendTCPData(Guid toClient, Packet packet)
        {
            packet.WriteLength();
            _dataSender.SendDataTCP(toClient, packet);
        }

        private void SendUDPData(Guid toClient, Packet packet)
        {
            packet.WriteLength();
            _dataSender.SendDataUDP(toClient, packet);
        }

        private void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in _gameRoomHolder.GetAll())
            {
                _dataSender.SendDataTCP(client.Id, packet);
            }
        }

        private void SendTCPDataToAll(Guid exceptClient, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in _gameRoomHolder.GetAll())
            {
                if (client.Id != exceptClient)
                {
                    _dataSender.SendDataTCP(client.Id, packet);
                }
            }
        }

        private void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in _gameRoomHolder.GetAll())
            {
                _dataSender.SendDataUDP(client.Id, packet);
            }
        }

        private void SendUDPDataToAll(Guid exceptClient, Packet packet)
        {
            packet.WriteLength();

            foreach (var client in _gameRoomHolder.GetAll())
            {
                if (client.Id != exceptClient)
                {
                    _dataSender.SendDataUDP(client.Id, packet);
                }
            }
        }
        #endregion

        public void GameRoomData(Guid toGameRoom)
        {
            using (Packet packet = new Packet(ToGameRoom.gameRoomData))
            {
                //sending real id
                packet.Write(toGameRoom);

                SendTCPData(toGameRoom, packet);
            }
        }
    }
}
