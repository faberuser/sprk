using System.Collections.Generic;
using System.Linq;

public sealed class SprkMissionHistoryState
{
	private string scope = "";

	private readonly HashSet<string> seen = new HashSet<string>();

	private readonly HashSet<string> pending = new HashSet<string>();

	public int Epoch { get; private set; }

	public int Revision { get; private set; }

	public int PendingCount => pending.Count;

	public bool SetScope(string value)
	{
		if (scope == value)
		{
			return false;
		}
		scope = value;
		Epoch++;
		Revision = 0;
		seen.Clear();
		pending.Clear();
		return true;
	}

	public bool Contains(string key)
	{
		return seen.Contains(key);
	}

	public void Mark(string key)
	{
		if (seen.Add(key))
		{
			pending.Add(key);
			Revision++;
		}
	}

	public string[] Batch()
	{
		return pending.ToArray();
	}

	public bool Merge(int epoch, IEnumerable<string> remote, IEnumerable<string> sent)
	{
		if (epoch != Epoch)
		{
			return false;
		}
		foreach (string item in remote)
		{
			if (seen.Add(item))
			{
				Revision++;
			}
		}
		foreach (string item2 in sent)
		{
			pending.Remove(item2);
		}
		return true;
	}
}
