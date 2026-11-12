using NGame2.NGuild;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DecoratorGuildRaid : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/WorldMap/Node/Decorator/DecoratorGuildRaid";

	public static PrefabAuto<DecoratorGuildRaid> PrefabAuto = new PrefabAuto<DecoratorGuildRaid>("Prefab2/WorldMap/Node/Decorator/DecoratorGuildRaid");

	public UIPanel Root;

	public UIWidget ClearRoot;

	public UIWidget HPRoot;

	public UISprite HPProgress;

	public void Open(int chapterIndex, int dungeonIndex)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(Root.gameObject, layerInfo.Layer);
		InitState(chapterIndex, dungeonIndex);
	}

	private void InitState(int chapterIndex, int dungeonIndex)
	{
		GuildRaidDungeonInfo guildRaidDungeonInfo = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.GetGuildRaidDungeonInfo(chapterIndex) : null);
		if (guildRaidDungeonInfo == null)
		{
			UIHelper.EnableGameObject(HPRoot, enable: false);
			UIHelper.EnableGameObject(ClearRoot, enable: false);
		}
		else if (dungeonIndex < guildRaidDungeonInfo.DungeonIndex)
		{
			UIHelper.EnableGameObject(HPRoot, enable: false);
			UIHelper.EnableGameObject(ClearRoot, enable: true);
		}
		else if (dungeonIndex == guildRaidDungeonInfo.DungeonIndex)
		{
			UIHelper.EnableGameObject(HPRoot, enable: true);
			UIHelper.EnableGameObject(ClearRoot, enable: false);
			long totalMaxHp = 0L;
			long totalRemainHp = 0L;
			long bossMaxHp = 0L;
			long bossRemainHp = 0L;
			GuildManager.GetGuildRaidHpInfo(UIHelper.GetMonsterInfoFromDungeon(chapterIndex, dungeonIndex, DungeonDifficultyType.Normal, isScenario: false, showDuplicated: true), guildRaidDungeonInfo, out totalMaxHp, out totalRemainHp, out bossMaxHp, out bossRemainHp);
			UIHelper.SetProgressValue(HPProgress, null, totalRemainHp, totalMaxHp);
		}
		else
		{
			UIHelper.EnableGameObject(HPRoot, enable: false);
			UIHelper.EnableGameObject(ClearRoot, enable: false);
		}
	}
}
