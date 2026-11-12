namespace NShared.NJit;

public interface IJitDictionaryRead<K, V>
{
	int Count { get; }

	bool TryGetValue(K key, out V value);
}
