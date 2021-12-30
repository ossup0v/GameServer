namespace GameServer.Network
{
    public interface IServerHandler
    {
        Task WelcomeReceived(Guid fromClient, Packet packet);
        Task RegisterUser(Guid fromClient, Packet packet);
        Task JoinGameRoom(Guid fromClient, Packet packet);
        Task LoginUser(Guid fromClient, Packet packet);
        Task CreateGameRoom(Guid fromClient, Packet packet);
    }
}