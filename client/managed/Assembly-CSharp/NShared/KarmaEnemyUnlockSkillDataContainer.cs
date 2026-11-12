using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KarmaEnemyUnlockSkillDataContainer : JitDataContainerDefault<KarmaEnemyUnlockSkillKey, KarmaEnemyUnlockSkillData>
{
	public const string TABLENAME = "KarmaEnemyUnlockSkillTable";

	private readonly Dictionary<int, SortedList<int, int[]>> _unlockSkillDict = new Dictionary<int, SortedList<int, int[]>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaEnemyUnlockSkillTable");

	protected override void OnEndLoad_Final()
	{
		foreach (KarmaEnemyUnlockSkillData allDatum in GetAllData())
		{
			if (!_unlockSkillDict.ContainsKey(allDatum.PunishmentGroupIndex))
			{
				_unlockSkillDict.Add(allDatum.PunishmentGroupIndex, new SortedList<int, int[]>());
			}
			_unlockSkillDict[allDatum.PunishmentGroupIndex].Add(allDatum.TriggerLevel, allDatum.UnlockSkills);
		}
	}

	public int[] GetUnlockSkills(int punishGroupIndex, int level)
	{
		if (!_unlockSkillDict.TryGetValue(punishGroupIndex, out var value))
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		foreach (KeyValuePair<int, int[]> item in value)
		{
			if (item.Key <= level)
			{
				return item.Value;
			}
		}
		return null;
	}
}
