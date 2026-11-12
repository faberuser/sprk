using System.Collections.Generic;
using System.Linq;

namespace NShared;

public class SerializedSnapshotContainer
{
	private Dictionary<ISnapshotObject, object> _serializedDictionary = new Dictionary<ISnapshotObject, object>();

	public void Add(ISnapshotObject snapshotObject, object serialized)
	{
		if (snapshotObject != null)
		{
			snapshotObject.GetUid();
			_serializedDictionary.Remove(snapshotObject);
			_serializedDictionary.Add(snapshotObject, serialized);
		}
	}

	public object GetSerialized(ISnapshotObject snapshotObject)
	{
		object value = null;
		_serializedDictionary.TryGetValue(snapshotObject, out value);
		return value;
	}

	public void Deserialize()
	{
		foreach (KeyValuePair<ISnapshotObject, object> item in _serializedDictionary)
		{
			ISnapshotObject key = item.Key;
			if (key != null)
			{
				object value = item.Value;
				key.Deserialize(value);
			}
		}
	}

	public List<ISnapshotObject> GetAllSnapshotList()
	{
		List<ISnapshotObject> list = _serializedDictionary.Keys.ToList();
		list.Sort((ISnapshotObject l, ISnapshotObject r) => l.GetUid().CompareTo(r.GetUid()));
		return list;
	}
}
