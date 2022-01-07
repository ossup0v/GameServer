using GameServer.Common;

namespace GameServer.NetworkWrappper.Holders
{
    public interface IHolder<TKey, TValue> 
        where TKey : struct 
        where TValue : class, IWithId<TKey>
    {
        TValue Get(TKey id);
        IEnumerable<TValue> GetAll();
        void Remove(TKey key);
        void AddNew(TValue @new);
        int Count { get; }
    }
}
