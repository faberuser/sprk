using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PunishmentRaidStateBattlePanel : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PunishmentRaidStateBattlePanel";

	public PunishmentRaidStatePanel BoardComponent;

	public GameObject Root;

	public void Open(int chapterIndex, int dungeonIndex)
	{
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon == null)
		{
			base.Open();
			return;
		}
		int raidIndex = raidDataByDungeon.Index;
		int raidLevel = raidDataByDungeon.Level;
		Open(delegate
		{
			BoardComponent.Open(raidIndex, raidLevel, Root);
		}, null);
	}

	public override void Open()
	{
		int chapter = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon.Chapter;
		int linkedDungeon = SingletonAuto<PunishmentRaidContext>.instance.EnterDungeon.LinkedDungeon;
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapter, linkedDungeon);
		if (raidDataByDungeon == null)
		{
			base.Open();
			return;
		}
		int raidIndex = raidDataByDungeon.Index;
		int raidLevel = raidDataByDungeon.Level;
		Open(delegate
		{
			BoardComponent.Open(raidIndex, raidLevel, Root);
		}, null);
	}
}
