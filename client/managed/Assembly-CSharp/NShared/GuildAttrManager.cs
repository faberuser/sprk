namespace NShared;

public class GuildAttrManager<TKey, TValue> : AccountAttrManager<TKey, TValue> where TValue : IAccountAttrKey<TKey>
{
}
