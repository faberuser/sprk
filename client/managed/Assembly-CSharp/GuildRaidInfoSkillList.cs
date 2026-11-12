using NGame2.NUI.NComponent2;
using NVespa.NPrefab2;

public class GuildRaidInfoSkillList : RaidInfoSkillList
{
	protected override PrefabAuto<RaidSkillComponent> _prefabAuto => RaidSkillComponent.GuildRaidPrefabAuto;
}
