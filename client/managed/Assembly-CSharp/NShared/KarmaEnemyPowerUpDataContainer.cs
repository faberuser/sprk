using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KarmaEnemyPowerUpDataContainer : JitDataContainerDefault<int, KarmaEnemyPowerUpData>
{
	public const string TABLENAME = "KarmaEnemyPowerUpTable";

	private JitDictionary<int, List<int>> _powerUpSkillDict = new JitDictionary<int, List<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaEnemyPowerUpTable");

	public KarmaEnemyPowerUpDataContainer()
	{
		base.Packer.RegisterAndBind(_powerUpSkillDict);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KarmaEnemyPowerUpData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_powerUpSkillDict.JsonLoad_AddToListValue(item.PunishmentGroupIndex, item.SkillIndex);
		}
	}

	public IList<int> GetAllEnemyPowerUpSkills(int punishGroupIndex)
	{
		return _powerUpSkillDict.GetOrDefault(punishGroupIndex);
	}
}
