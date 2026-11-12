using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DungeonMonstersInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Dungeon/DungeonMonstersInfoPopup";

	public UIScrollGrid MonsterGrid;

	private EnterDungeonData _enterDungeon;

	private Func<int, long, long> _hpGetter;

	private List<Component> _monsterIconList = new List<Component>();

	public static void Open(EnterDungeonData enterDungeon, Func<int, long, long> hpGetter = null)
	{
		WindowPrefab<DungeonMonstersInfoPopup> windowPrefab = new WindowPrefab<DungeonMonstersInfoPopup>("Resources/Prefab2/Window/Dungeon/DungeonMonstersInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(enterDungeon, hpGetter);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(EnterDungeonData enterDungeon, Func<int, long, long> hpGetter)
	{
		Open(delegate
		{
			_enterDungeon = enterDungeon;
			_hpGetter = hpGetter;
			SetupMonsters();
		}, null);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	private void Clear()
	{
		ClearMonsterIcon();
	}

	protected void ClearMonsterIcon()
	{
		FactoryBase.ReleaseAndClearList(_monsterIconList);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}

	private void SetupMonsters()
	{
		if (MonsterGrid == null)
		{
			return;
		}
		ClearMonsterIcon();
		int questMonsterIndex = _enterDungeon.QuestDungeon?.MonsterIndex ?? 0;
		List<Component> list = UIHelper.CreateMonsterPortraitList(MonsterGrid, _enterDungeon, questMonsterIndex, isShowHP: true);
		if (__Extension_Method_ICollection__.COUNT(list) > 0)
		{
			_monsterIconList.AddRange(list);
		}
		foreach (Component monsterIcon in _monsterIconList)
		{
			MonsterComponent monsterComponent = monsterIcon as MonsterComponent;
			MonsterHeroInfo info = monsterComponent.Info;
			if (info != null)
			{
				int waveSlotIndex = info.WaveSlotIndex;
				long maxHp = info.GetMaxHp();
				long hp = Vespa_System_Action_Extension_Method.fire(_hpGetter, waveSlotIndex, maxHp);
				monsterComponent.SetHP(hp, maxHp);
			}
		}
	}
}
