using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NVespa;

namespace NShared;

public static class SnapshotDumper
{
	private class DumpedMemberData
	{
		public long Uid { get; set; }

		public string Value { get; set; }

		public string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Uid > 0)
			{
				stringBuilder.Append(Uid);
			}
			else if (Value != null)
			{
				stringBuilder.Append("\"");
				stringBuilder.Append(Value);
				stringBuilder.Append("\"");
			}
			else
			{
				stringBuilder.Append("null");
			}
			return stringBuilder.ToString();
		}

		public static DumpedMemberData Create(object elem)
		{
			if (elem == null)
			{
				return new DumpedMemberData
				{
					Uid = 0L,
					Value = null
				};
			}
			if (elem is ISnapshotObject)
			{
				return new DumpedMemberData
				{
					Uid = (elem as ISnapshotObject).GetUid()
				};
			}
			return new DumpedMemberData
			{
				Value = elem.ToString()
			};
		}
	}

	public static DumpedSnapshotContainer CreateDumpedSnapshotContainer(BattleInstance battleInstance)
	{
		DumpedSnapshotContainer dumpedSnapshotContainer = new DumpedSnapshotContainer();
		foreach (BattleCreature allCreature in battleInstance.CreatureContainer.GetAllCreatures())
		{
			dumpedSnapshotContainer.Add(allCreature);
		}
		dumpedSnapshotContainer.Add(battleInstance.Snapshot);
		return dumpedSnapshotContainer;
	}

	public static string Dump(ISnapshotObject o, IList s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{ ");
		stringBuilder.Append("\"Type\": \"");
		stringBuilder.Append(o.GetTypeName());
		stringBuilder.Append("\", \"Uid\": ");
		stringBuilder.Append(o.GetUid());
		stringBuilder.Append(", \"Dumped\": ");
		stringBuilder.Append("[");
		if (s != null)
		{
			for (int i = 0; i < s.Count; i++)
			{
				DumpedMemberData dumpedMemberData = DumpedMemberData.Create(s[i]);
				stringBuilder.Append((i == 0) ? "" : ",");
				stringBuilder.Append(dumpedMemberData.Dump());
			}
		}
		stringBuilder.Append("]");
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}

	public static DumpedSnapshotContainer Parse(string[] lines)
	{
		if (lines == null)
		{
			return null;
		}
		DumpedSnapshotContainer dumpedSnapshotContainer = new DumpedSnapshotContainer();
		int i = 0;
		string text = null;
		try
		{
			for (; i < lines.Length; i++)
			{
				text = lines[i];
				if (string.IsNullOrEmpty(text) || !(Json.Deserialize(text) is Dictionary<string, object> dictionary))
				{
					continue;
				}
				string text2 = (string)dictionary["Type"];
				int num = BaseJsonMarshaler.To<int>(dictionary["Uid"]);
				List<object> list = dictionary["Dumped"] as List<object>;
				ISnapshotObject snapshotObject = SnapshotFactory.Create(text2);
				if (snapshotObject == null)
				{
					VespaDebug.LogError("Created snapshot is null : type=" + text2);
					continue;
				}
				snapshotObject.SetUid(num);
				List<string> dumped = null;
				if (list != null)
				{
					dumped = list.ConvertAll((object e) => (string)e);
				}
				dumpedSnapshotContainer.AddDumped(snapshotObject, dumped);
			}
			dumpedSnapshotContainer.Parse();
			return dumpedSnapshotContainer;
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to parse lineNo : " + i + " line=" + text + "\n" + ex);
			return null;
		}
	}
}
