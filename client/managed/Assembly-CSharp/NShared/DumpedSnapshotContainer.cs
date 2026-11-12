using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NShared;

public class DumpedSnapshotContainer
{
	private Dictionary<long, ISnapshotObject> _snapshotDictionary = new Dictionary<long, ISnapshotObject>();

	private Dictionary<ISnapshotObject, IList<string>> _dumpedDictionary = new Dictionary<ISnapshotObject, IList<string>>();

	public void Add(ISnapshotObject snapshotObject)
	{
		if (snapshotObject != null)
		{
			long uid = snapshotObject.GetUid();
			_snapshotDictionary.Remove(uid);
			_snapshotDictionary.Add(uid, snapshotObject);
		}
	}

	public void AddDumped(ISnapshotObject snapshotObject, IList<string> dumped)
	{
		if (snapshotObject != null)
		{
			long uid = snapshotObject.GetUid();
			_snapshotDictionary.Remove(uid);
			_snapshotDictionary.Add(uid, snapshotObject);
			_dumpedDictionary.Remove(snapshotObject);
			_dumpedDictionary.Add(snapshotObject, dumped);
		}
	}

	public object GetSnapshot(long uid)
	{
		ISnapshotObject value = null;
		_snapshotDictionary.TryGetValue(uid, out value);
		return value;
	}

	public void Parse()
	{
		using (new SnapshotWritableBlock())
		{
			foreach (KeyValuePair<ISnapshotObject, IList<string>> item in _dumpedDictionary)
			{
				ISnapshotObject key = item.Key;
				if (key != null)
				{
					IList<string> value = item.Value;
					key.Parse(value, this);
				}
			}
		}
	}

	public string Dump()
	{
		PrepareDump();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<long, ISnapshotObject> item in _snapshotDictionary)
		{
			ISnapshotObject value = item.Value;
			if (value != null)
			{
				string value2 = value.Dump();
				stringBuilder.AppendLine(value2);
			}
		}
		return stringBuilder.ToString();
	}

	private void PrepareDump()
	{
		ISnapshotObject[] array = _snapshotDictionary.Values.ToArray();
		foreach (ISnapshotObject snapshotObject in array)
		{
			PrepareDumpInternal(snapshotObject);
		}
	}

	private void PrepareDumpInternal(ISnapshotObject snapshotObject)
	{
		if (snapshotObject == null)
		{
			return;
		}
		IList<ISnapshotObject> children = snapshotObject.GetChildren();
		if (children == null || children.Count == 0)
		{
			return;
		}
		foreach (ISnapshotObject item in children)
		{
			if (!_snapshotDictionary.ContainsKey(item.GetUid()))
			{
				Add(item);
				PrepareDumpInternal(item);
			}
		}
	}

	public List<ISnapshotObject> GetAllSnapshotList()
	{
		List<ISnapshotObject> list = _snapshotDictionary.Values.ToList();
		list.Sort((ISnapshotObject l, ISnapshotObject r) => l.GetUid().CompareTo(r.GetUid()));
		return list;
	}
}
