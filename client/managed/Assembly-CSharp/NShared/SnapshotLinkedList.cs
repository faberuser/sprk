using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotLinkedList<T> : ISnapshotObject, IEnumerable<T>, IEnumerable
{
	private long _uid;

	private readonly LinkedList<T> _list;

	public SnapshotRefCounter RefCounter { get; private set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	public LinkedListNode<T> First => _list.First;

	public int Count => GetCount();

	static SnapshotLinkedList()
	{
		SnapshotFactory.Register<SnapshotLinkedList<T>>(GetTypeName(typeof(T)));
	}

	public SnapshotLinkedList()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
		_list = new LinkedList<T>();
	}

	public void AddFirst(T value)
	{
		_list.AddFirst(value);
		SnapshotUtility.IncRefCount(value);
	}

	public void AddLast(T value)
	{
		_list.AddLast(value);
		SnapshotUtility.IncRefCount(value);
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

	public void Remove(LinkedListNode<T> node)
	{
		_list.Remove(node);
		SnapshotUtility.DecRefCount(node.Value);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _list.GetEnumerator();
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
		return "SnapshotLinkedList_" + type.Name;
	}

	public string GetTypeName()
	{
		return GetTypeName(typeof(T));
	}

	void ISnapshotObject.OnRelease()
	{
		Clear();
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
			AddLast(item);
		}
	}

	string ISnapshotObject.Dump()
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<T>();
		if (impl == null)
		{
			VespaDebug.LogError("SnapshotLinkedList Dump : imple not found : " + typeof(T).Name);
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
			VespaDebug.LogError("SnapshotLinkedList Parse : imple not found : " + typeof(T).Name);
		}
		else if (dumped != null)
		{
			for (int i = 0; i < dumped.Count; i++)
			{
				string value = dumped[i];
				object obj = impl.From(value, container);
				AddLast((T)obj);
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
