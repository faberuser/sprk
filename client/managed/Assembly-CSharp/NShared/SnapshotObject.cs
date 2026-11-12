using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotObject : ISnapshotObject, ISnapshotParent
{
	private readonly List<ISnapshotMember> _memberList = new List<ISnapshotMember>();

	private long _uid;

	public SnapshotRefCounter RefCounter { get; private set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	static SnapshotObject()
	{
	}

	public SnapshotObject()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
	}

	public void AddMember(ISnapshotMember member)
	{
		_memberList.Add(member);
	}

	public ISnapshotMember[] GetMembers()
	{
		return _memberList.ToArray();
	}

	public long GetUid()
	{
		return _uid;
	}

	public void SetUid(long uid)
	{
		_uid = uid;
	}

	public string GetTypeName()
	{
		return GetType().Name;
	}

	void ISnapshotObject.OnRelease()
	{
		ResetMembers();
	}

	protected void ResetMembers()
	{
		for (int i = 0; i < _memberList.Count; i++)
		{
			_memberList[i].Reset();
		}
	}

	public object Serialize(SerializedSnapshotContainer container)
	{
		if (container != null)
		{
			object serialized = container.GetSerialized(this);
			if (serialized != null)
			{
				return serialized;
			}
		}
		List<object> list = new List<object>(_memberList.Count);
		container?.Add(this, list);
		foreach (ISnapshotMember member in _memberList)
		{
			object obj = member.Get();
			list.Add(obj);
			ISnapshotObject snapshotObject = obj as ISnapshotObject;
			if (container != null)
			{
				snapshotObject?.Serialize(container);
			}
		}
		return list;
	}

	public virtual void Deserialize(object s)
	{
		if (s is List<object> list && list.Count == _memberList.Count)
		{
			for (int i = 0; i < list.Count; i++)
			{
				object value = list[i];
				_memberList[i]?.Set(value);
			}
		}
	}

	string ISnapshotObject.Dump()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < _memberList.Count; i++)
		{
			ISnapshotMember snapshotMember = _memberList[i];
			object value = snapshotMember.Get();
			string item = snapshotMember.GetSerializer().To(value);
			list.Add(item);
		}
		return SnapshotDumper.Dump(this, list);
	}

	void ISnapshotObject.Parse(IList<string> dumped, DumpedSnapshotContainer container)
	{
		if (dumped == null)
		{
			VespaDebug.LogError("Dumped is null. type=" + GetType().Name);
			return;
		}
		if (dumped.Count != _memberList.Count)
		{
			VespaDebug.LogError("Dumped member count mismatch. type=" + GetType().Name + " dumped=" + dumped.Count + " member=" + _memberList.Count);
			return;
		}
		for (int i = 0; i < dumped.Count; i++)
		{
			string value = dumped[i];
			ISnapshotMember snapshotMember = _memberList[i];
			if (snapshotMember != null)
			{
				ISnapshotMemberSerializer serializer = snapshotMember.GetSerializer();
				if (serializer != null)
				{
					object value2 = serializer.From(value, container);
					snapshotMember.Set(value2);
				}
			}
		}
	}

	IList<ISnapshotObject> ISnapshotObject.GetChildren()
	{
		List<ISnapshotObject> list = new List<ISnapshotObject>();
		for (int i = 0; i < _memberList.Count; i++)
		{
			ISnapshotMember snapshotMember = _memberList[i];
			if (snapshotMember != null && snapshotMember.Get() is ISnapshotObject)
			{
				list.Add(snapshotMember.Get() as ISnapshotObject);
			}
		}
		return list;
	}
}
