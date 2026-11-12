namespace NShared;

public interface IReadWriter
{
}
public interface IReadWriter<K> : IReadWriter
{
	string ToString(K key);

	K FromString(string value);
}
