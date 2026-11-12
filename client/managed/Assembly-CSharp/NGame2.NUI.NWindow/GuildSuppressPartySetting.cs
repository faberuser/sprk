using NGame2.NParty;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildSuppressPartySetting : RaidPartySetting
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Raid/GuildSuppressPartySetting";

	private GuildSuppressDungeonData _guildSuppressDungeonData;

	public GuildSuppressDungeonData GuildSuppressDungeonData => _guildSuppressDungeonData;

	public void Init(Listener listener, IDataProvider dataProvider, RaidData raidData, GuildSuppressDungeonData guildSuppressDungeonData)
	{
		InitForRaid(listener, dataProvider, raidData);
		_guildSuppressDungeonData = guildSuppressDungeonData;
		GlobalAutoBehaviour<PartyManager>.instance.SetGuildSuppress(_guildSuppressDungeonData.Index, raidData.Index, raidData.Level);
	}
}
