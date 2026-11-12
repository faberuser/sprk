namespace NShared;

public interface IDataKeySelector<K>
{
	K Key { get; }
}
