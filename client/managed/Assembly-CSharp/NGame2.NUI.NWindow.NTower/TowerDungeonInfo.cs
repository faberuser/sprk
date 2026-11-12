using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class TowerDungeonInfo : BattleDungeonInfo
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Tower/TowerDungeonInfo";

	public UILabel EnemyNameLabel;

	public UILabel EnemyLevelLabel;

	public Transform[] FloorType;

	public CachedAccountInfo NpcInfo { get; private set; }

	public void Init(EnterDungeonData enterDungeon, CachedAccountInfo npcInfo, int floorTypeIndex, Listener listener)
	{
		NpcInfo = npcInfo;
		OnOpen(enterDungeon, listener);
		Refresh();
		string text = ((npcInfo != null) ? npcInfo.UserInfo.Nick : "Unknown");
		int num = npcInfo?.UserInfo.TeamLevel ?? 1;
		UIHelper.SetLabelText(EnemyNameLabel, text);
		UIHelper.SetLabelText(EnemyLevelLabel, num);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FloorType); i++)
		{
			UIHelper.EnableGameObject(FloorType[i], i == floorTypeIndex);
		}
	}

	public override void SetupMonsterInfos()
	{
		if (NpcInfo == null)
		{
			base.SetupMonsterInfos();
			return;
		}
		InfoComponents.ReleaseMonsterComponents();
		foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in NpcInfo.HeroInfos)
		{
			_ = heroInfo.Key;
			CachedHeroInfo value = heroInfo.Value;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(InfoComponents.Grid_Monsters, HeroPortraitComponent.ShortPrefabAuto);
			if (!(heroPortraitComponent == null) && heroPortraitComponent.Open(value, null))
			{
				InfoComponents.AddMonsterComponent(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(InfoComponents.Grid_Monsters);
	}
}
