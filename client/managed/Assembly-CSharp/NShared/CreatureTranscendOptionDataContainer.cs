using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class CreatureTranscendOptionDataContainer : JitDataContainerDefault<int, CreatureTranscendOptionData>
{
	public const string TABLENAME = "CreatureTranscendOptionTable";

	private Dictionary<int, int[]> _lookup;

	private Dictionary<int, int[]> _pointLookup;

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureTranscendOptionTable");

	protected override void OnEndLoad_Final()
	{
		base.OnEndLoad_Final();
		Dictionary<int, List<int>> keyLists_by_TranscendLevel = new Dictionary<int, List<int>>();
		ForEachData(delegate(CreatureTranscendOptionData creatureTranscendOptionData)
		{
			if (!keyLists_by_TranscendLevel.TryGetValue(creatureTranscendOptionData.TranscendLevel, out var value3))
			{
				value3 = new List<int>();
				keyLists_by_TranscendLevel.Add(creatureTranscendOptionData.TranscendLevel, value3);
			}
			value3.Add(creatureTranscendOptionData.GETKEY());
		});
		_lookup = new Dictionary<int, int[]>();
		_pointLookup = new Dictionary<int, int[]>();
		foreach (KeyValuePair<int, List<int>> item in keyLists_by_TranscendLevel)
		{
			int key = item.Key;
			List<int> value = item.Value;
			value.Sort();
			_lookup.Add(key, value.ToArray());
			int[] value2 = value.Select((int key2) => GetData(key2).ReqTranscendPoint).ToArray();
			_pointLookup.Add(key, value2);
		}
	}

	public CreatureTranscendOptionData GetData(int transcendLevel, int slotIndex)
	{
		return GetData(transcendLevel * 10 + slotIndex);
	}

	public CreatureData.TranscendSkill GetTranscendSkillData(int transcended, int index)
	{
		CreatureTranscendOptionData data = GetData(transcended, index);
		if (data == null)
		{
			return null;
		}
		return new CreatureData.TranscendSkill
		{
			Transcended = transcended,
			Index = index,
			TranscendSkillIndex = data.LegacySkillIndex,
			ReqSKillPoint = data.ReqTranscendPoint,
			Level = 0,
			MaxLevel = data.MaxLevel
		};
	}

	public int[] GetSkillList(int transcendLevel)
	{
		_lookup.TryGetValue(transcendLevel, out var value);
		return value;
	}

	public int[] GetPointList(int transcendLevel)
	{
		_pointLookup.TryGetValue(transcendLevel, out var value);
		return value;
	}
}
