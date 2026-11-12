using System;
using System.Collections.Generic;
using MiniJSON;

namespace NShared;

public class BattleInstanceDump
{
	private readonly List<string> _dumpedList = new List<string>();

	public BattleInstanceDump(BattleInstance instance)
	{
		if (instance == null)
		{
			return;
		}
		Queue<ISnapshotObject> queue = new Queue<ISnapshotObject>();
		queue.Enqueue(instance.Snapshot);
		foreach (BattleCreature allCreature in instance.CreatureContainer.GetAllCreatures())
		{
			queue.Enqueue(allCreature);
		}
		HashSet<ISnapshotObject> hashSet = new HashSet<ISnapshotObject>();
		while (queue.Count > 0)
		{
			ISnapshotObject snapshotObject = queue.Dequeue();
			EnqueueChildSnapshotObject(snapshotObject, queue, hashSet);
		}
		_dumpedList.Clear();
		foreach (ISnapshotObject item2 in hashSet)
		{
			string item = item2.Dump();
			_dumpedList.Add(item);
		}
	}

	private void EnqueueChildSnapshotObject(ISnapshotObject snapshotObject, Queue<ISnapshotObject> queue, HashSet<ISnapshotObject> collected)
	{
		if (snapshotObject == null || collected.Contains(snapshotObject))
		{
			return;
		}
		queue.Enqueue(snapshotObject);
		collected.Add(snapshotObject);
		IList<ISnapshotObject> children = snapshotObject.GetChildren();
		if (children == null)
		{
			return;
		}
		foreach (ISnapshotObject item in children)
		{
			EnqueueChildSnapshotObject(item, queue, collected);
		}
	}

	public bool IsSame(BattleInstanceDump other)
	{
		if (_dumpedList.Count != other._dumpedList.Count)
		{
			return false;
		}
		Dictionary<long, long> dictionary = new Dictionary<long, long>();
		for (int i = 0; i < _dumpedList.Count; i++)
		{
			string json = _dumpedList[i];
			string json2 = other._dumpedList[i];
			Dictionary<string, object> dictionary2 = Json.Deserialize(json) as Dictionary<string, object>;
			Dictionary<string, object> obj = Json.Deserialize(json2) as Dictionary<string, object>;
			long num = (long)dictionary2["Uid"];
			long num2 = (long)obj["Uid"];
			if (num != num2)
			{
				dictionary[num] = num2;
			}
		}
		foreach (KeyValuePair<long, long> item in dictionary)
		{
			long key = item.Key;
			long value = item.Value;
			string oldValue = "\"Uid\": " + value + ",";
			string newValue = "\"Uid\": " + key + ",";
			string oldValue2 = "\"U:" + value + "\"";
			string newValue2 = "\"U:" + key + "\"";
			for (int j = 0; j < other._dumpedList.Count; j++)
			{
				string text = other._dumpedList[j];
				text = text.Replace(oldValue, newValue);
				text = text.Replace(oldValue2, newValue2);
				other._dumpedList[j] = text;
			}
		}
		for (int k = 0; k < _dumpedList.Count; k++)
		{
			string a = _dumpedList[k];
			string b = other._dumpedList[k];
			if (!string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
		}
		return true;
	}
}
