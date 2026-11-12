namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleSimulationAccountInfo : CachedAccountInfo
{
	public GuildSkillDataBase[] GuildSkills { get; set; }

	public AccountBuffDataBase[] AccountBuffs { get; set; }

	public ClassBuffDataBase[] ClassBuffDataBases { get; set; }

	public DefaultExtraStatData[] ExtraStatDatas { get; set; }

	public PetStatDataBase[] PetStatDataBases { get; set; }
}
