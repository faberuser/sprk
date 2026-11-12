namespace NShared.NJit;

public interface IGetData_for_GMS<K, V> : IGetAllData_for_GMS<V>
{
	V GetData(K key);
}
