namespace GameServer.Common
{
    public interface IWithId<TId> where TId : struct
    {
        TId Id { get; }
    }
}
