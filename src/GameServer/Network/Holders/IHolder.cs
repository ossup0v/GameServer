namespace GameServer.Network.Holders
{
    public interface IHolder<TValue, TKey> where TValue : class where TKey : struct
    {
        TValue? Get(TKey id);
        IEnumerable<TValue> GetAll();
        void AddNew(TValue @new);
    }
}
