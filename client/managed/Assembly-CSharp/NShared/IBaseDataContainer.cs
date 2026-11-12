using System.Collections;

namespace NShared;

public interface IBaseDataContainer<K, V> : IBaseDataContainer
{
	V GetData(K key);
}
public interface IBaseDataContainer
{
	string TableDataPath { get; }

	bool IsLoaded { get; }

	string LoadError { get; }

	IEnumerable GetAllData();

	object GetData(object key);
}
