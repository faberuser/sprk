namespace NVespa;

internal static class EmptyIncreaseDicHolder<TKey, TValue>
{
	internal static IncreaseDic<TKey, TValue> Empty;

	static EmptyIncreaseDicHolder()
	{
		Empty = new IncreaseDic<TKey, TValue>();
	}
}
