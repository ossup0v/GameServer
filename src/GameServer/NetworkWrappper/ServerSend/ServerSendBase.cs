using GameServer.Common;
using GameServer.Network;
using GameServer.NetworkWrappper.Holders;
using GameServer.NetworkWrappper.NetworkProcessors;

namespace GameServer.NetworkWrappper
{
    public abstract class ServerSendBase<THolder, TKey, TValue>
        where THolder : IHolder<TKey, TValue>
        where TKey : struct, IComparable<TKey>
        where TValue : class, IWithId<TKey>
    {
        protected readonly IDataSender<TKey> DataSender;
        protected readonly THolder Holder;

        public ServerSendBase(THolder holder, IDataSender<TKey> dataSender)
        {
            Holder = holder;
            DataSender = dataSender;
        }

        protected void SendTCPData(TKey toClient, Packet packet)
        {
            packet.WriteLength();
            DataSender.SendDataTCP(toClient, packet);
        }

        protected void SendUDPData(TKey toClient, Packet packet)
        {
            packet.WriteLength();
            DataSender.SendDataUDP(toClient, packet);
        }

        protected void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Holder.GetAll())
            {
                DataSender.SendDataTCP(client.Id, packet);
            }
        }

        protected void SendTCPDataToAll(TKey exceptClient, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Holder.GetAll())
            {
                if (!client.Id.IsValueEquals(exceptClient))
                {
                    DataSender.SendDataTCP(client.Id, packet);
                }
            }
        }

        protected void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Holder.GetAll())
            {
                DataSender.SendDataUDP(client.Id, packet);
            }
        }

        protected void SendUDPDataToAll(TKey exceptClient, Packet packet)
        {
            packet.WriteLength();

            foreach (var client in Holder.GetAll())
            {
                if (client.Id.IsValueEquals(exceptClient))
                {
                    DataSender.SendDataUDP(client.Id, packet);
                }
            }
        }
    }
}
