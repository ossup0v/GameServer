using GameServer.Common;

namespace GameServer.NetworkWrappper.Holders
{
    public interface IHolder<TKey, TValue> 
        where TKey : struct 
        where TValue : class, IWithId<TKey>
    {
        TValue? Get(TKey id);
        IEnumerable<TValue> GetAll();
        void AddNew(TValue @new);
        int Count { get; }
    }
}
