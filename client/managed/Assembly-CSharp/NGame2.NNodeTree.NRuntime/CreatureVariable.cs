using NGame2.NNodeTree.NRuntime.NVariable;
using NVespa;

namespace NGame2.NNodeTree.NRuntime;

public class CreatureVariable : SmallPooling<CreatureVariable>, ISmallPoolingItem
{
	private StringKeyDictionary<Tuple<IVariableValueBase, float>> _valueDic = new StringKeyDictionary<Tuple<IVariableValueBase, float>>();

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		_valueDic.Clear();
	}

	public void AddValue<T>(StringKey key, T value, float lifeTime)
	{
		IVariableValueBase i_item = ValueGenericMember<T>.Create(value);
		float i_item2 = MainTimer.time + lifeTime;
		_valueDic.ADD(key, Tuple.Create(i_item, i_item2));
	}

	public void AddValueBase(StringKey name, IVariableValueBase valueBase, float lifeTime)
	{
		float i_item = MainTimer.time + lifeTime;
		_valueDic.ADD(name, Tuple.Create(valueBase, i_item));
	}

	public bool RemoveValue(StringKey name)
	{
		if (name.IsNullOrEmpty())
		{
			return false;
		}
		return _valueDic.Remove(name);
	}

	public IVariableValueBase GetValueBase(StringKey key)
	{
		if (key.IsNullOrEmpty())
		{
			return null;
		}
		if (!_valueDic.TryGetValue(key, out var value))
		{
			return null;
		}
		if (value.Item2 < MainTimer.time)
		{
			_valueDic.Remove(key);
			return null;
		}
		return value.Item1;
	}
}
