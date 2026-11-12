using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotList<T> : ISnapshotObject, IEnumerable<T>, IEnumerable
{
	private long _uid;

	private readonly List<T> _list;

	public SnapshotRefCounter RefCounter { get; private set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	public T this[int index]
	{
		get
		{
			return _list[index];
		}
		set
		{
			SnapshotUtility.DecRefCount(_list[index]);
			_list[index] = value;
			SnapshotUtility.IncRefCount(value);
		}
	}

	public int Count => GetCount();

	static SnapshotList()
	{
		SnapshotFactory.Register<SnapshotList<T>>(GetTypeName(typeof(T)));
	}

	public SnapshotList()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
		_list = new List<T>();
	}

	public void Add(T value)
	{
		_list.Add(value);
		SnapshotUtility.IncRefCount(value);
	}

	public void AddRange(IEnumerable<T> values)
	{
		_list.AddRange(values);
		SnapshotUtility.IncRefCountList(values);
	}

	public void Clear()
	{
		SnapshotUtility.DecRefCountList(_list);
		_list.Clear();
	}

	public int GetCount()
	{
		return _list.Count;
	}

	public bool Contains(T value)
	{
		return _list.Contains(value);
	}

	public bool Remove(T value)
	{
		if (_list.Remove(value))
		{
			SnapshotUtility.DecRefCount(value);
			return true;
		}
		return false;
	}

	public bool Remove(Predicate<T> match)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			T val = _list[i];
			if (match(val))
			{
				_list.RemoveAt(i);
				SnapshotUtility.DecRefCount(val);
				return true;
			}
		}
		return false;
	}

	public bool RemoveAt(int index)
	{
		if (index >= _list.Count)
		{
			return false;
		}
		T value = _list[index];
		_list.RemoveAt(index);
		SnapshotUtility.DecRefCount(value);
		return true;
	}

	public void RemoveRange(int begin, int count)
	{
		for (int i = 0; i < count; i++)
		{
			int num = begin + i;
			if (num >= GetCount())
			{
				break;
			}
			SnapshotUtility.DecRefCount(_list[num]);
		}
		_list.RemoveRange(begin, count);
	}

	public virtual void Sort(Comparison<T> comparison)
	{
		_list.Sort(comparison);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	void ISnapshotObject.OnRelease()
	{
		Clear();
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
		return "SnapshotList_" + type.Name;
	}

	public string GetTypeName()
	{
		return GetTypeName(typeof(T));
	}

	object ISnapshotObject.Serialize(SerializedSnapshotContainer container)
	{
		object serialized = container.GetSerialized(this);
		if (serialized != null)
		{
			return serialized;
		}
		List<T> list = new List<T>(Count);
		container.Add(this, list);
		foreach (T item in (IEnumerable<T>)this)
		{
			list.Add(item);
			if (item is ISnapshotObject snapshotObject)
			{
				snapshotObject.Serialize(container);
			}
		}
		return list;
	}

	void ISnapshotObject.Deserialize(object s)
	{
		if (!(s is List<T> list))
		{
			return;
		}
		Clear();
		foreach (T item in list)
		{
			Add(item);
		}
	}

	string ISnapshotObject.Dump()
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<T>();
		if (impl == null)
		{
			VespaDebug.LogError("SnapshotList Dump : imple not found : " + typeof(T).Name);
			return null;
		}
		List<string> list = new List<string>();
		foreach (T item2 in (IEnumerable<T>)this)
		{
			string item = impl.To(item2);
			list.Add(item);
		}
		return SnapshotDumper.Dump(this, list);
	}

	void ISnapshotObject.Parse(IList<string> dumped, DumpedSnapshotContainer container)
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<T>();
		if (impl == null)
		{
			VespaDebug.LogError("SnapshotList Parse : imple not found : " + typeof(T).Name);
		}
		else if (dumped != null)
		{
			for (int i = 0; i < dumped.Count; i++)
			{
				string value = dumped[i];
				object obj = impl.From(value, container);
				Add((T)obj);
			}
		}
	}

	IList<ISnapshotObject> ISnapshotObject.GetChildren()
	{
		if (_list.Count > 0 && typeof(ISnapshotObject).IsAssignableFrom(typeof(T)))
		{
			List<ISnapshotObject> list = new List<ISnapshotObject>();
			{
				foreach (T item in _list)
				{
					list.Add((ISnapshotObject)(object)item);
				}
				return list;
			}
		}
		return null;
	}
}
