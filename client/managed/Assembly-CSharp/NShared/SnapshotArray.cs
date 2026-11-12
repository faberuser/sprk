using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotArray<T> : ISnapshotObject
{
	private long _uid;

	private T[] _values;

	private static T[] _emptyValues;

	public SnapshotRefCounter RefCounter { get; private set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	public T this[int i] => _values[i];

	static SnapshotArray()
	{
		_emptyValues = new T[0];
		SnapshotFactory.Register<SnapshotArray<T>>(GetTypeName(typeof(T)));
	}

	public SnapshotArray()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
		Set(_emptyValues);
	}

	public SnapshotArray(T[] values)
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
		Set(values);
	}

	public T[] Get()
	{
		return _values;
	}

	public void Set(T[] values)
	{
		if (values != _values)
		{
			SnapshotUtility.IncRefCountList(values);
			SnapshotUtility.DecRefCountList(_values);
			_values = values;
		}
	}

	public static implicit operator SnapshotArray<T>(T[] values)
	{
		return new SnapshotArray<T>(values);
	}

	public static implicit operator T[](SnapshotArray<T> array)
	{
		return array?._values;
	}

	public long GetUid()
	{
		return _uid;
	}

	public void SetUid(long uid)
	{
		_uid = uid;
	}

	public static string GetTypeName(Type type)
	{
		return "SnapshotArray_" + type.Name;
	}

	public string GetTypeName()
	{
		return GetTypeName(typeof(T));
	}

	object ISnapshotObject.Serialize(SerializedSnapshotContainer container)
	{
		object obj = container?.GetSerialized(this);
		if (obj != null)
		{
			return obj;
		}
		int num = ((_values != null) ? _values.Length : 0);
		T[] array = new T[num];
		container?.Add(this, array);
		for (int i = 0; i < num; i++)
		{
			array[i] = _values[i];
		}
		return array;
	}

	void ISnapshotObject.Deserialize(object s)
	{
		if (!(s is T[] values))
		{
			VespaDebug.LogError("Failed to get serialized array : s=" + s);
		}
		else
		{
			_values = values;
		}
	}

	void ISnapshotObject.OnRelease()
	{
		Set(_emptyValues);
	}

	string ISnapshotObject.Dump()
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<T>();
		if (impl == null)
		{
			VespaDebug.LogError("SnapshotList Array : impl not found : " + typeof(T).Name);
			return null;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < _values.Length; i++)
		{
			T val = _values[i];
			string item = impl.To(val);
			list.Add(item);
		}
		return SnapshotDumper.Dump(this, list);
	}

	void ISnapshotObject.Parse(IList<string> dumped, DumpedSnapshotContainer container)
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<T>();
		if (impl == null)
		{
			VespaDebug.LogError("SnapshotList Array : imple not found : " + typeof(T).Name);
		}
		else if (dumped != null)
		{
			List<T> list = new List<T>();
			for (int i = 0; i < dumped.Count; i++)
			{
				string value = dumped[i];
				object obj = impl.From(value, container);
				list.Add((T)obj);
			}
			_values = list.ToArray();
		}
	}

	IList<ISnapshotObject> ISnapshotObject.GetChildren()
	{
		if (_values != null && _values.Length != 0 && typeof(ISnapshotObject).IsAssignableFrom(typeof(T)))
		{
			List<ISnapshotObject> list = new List<ISnapshotObject>();
			for (int i = 0; i < _values.Length; i++)
			{
				list.Add((ISnapshotObject)(object)_values[i]);
			}
			return list;
		}
		return null;
	}
}
