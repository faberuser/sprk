using NShared.NJit;

namespace NShared;

public class SoulWeaponSkillDataContainer : JitDataContainerDefault<string, SoulWeaponSkillData>
{
	public const string TABLENAME = "SoulWeaponSkillTable";

	private JitDictionary<int, SoulWeaponSkillData> _datas_by_SkillIndex = new JitDictionary<int, SoulWeaponSkillData>();

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponSkillTable");

	public SoulWeaponSkillDataContainer()
	{
		base.Packer.RegisterAndBind(_datas_by_SkillIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (SoulWeaponSkillData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_datas_by_SkillIndex.JsonLoad_Get()[item.BasicSkillIndex] = item;
		}
	}

	public SoulWeaponSkillData GetDataByBasicSkillIndex(int skillIndex)
	{
		return _datas_by_SkillIndex.GetOrDefault(skillIndex);
	}
}
