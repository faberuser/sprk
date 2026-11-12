using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPet;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

internal class IllustratedBookManagement : BaseManagement<NGame2.NUI.NManager.IllustratedBookManagement>, IllustratedBookList.Listener, HeroListBook.Listener, HeroBook.Listener, ItemListBook.Listener, ItemBook.Listener, MonsterListBook.Listener, ChapterMonsterBook.Listener, BossMonsterInfoBook.Listener, Hero3DViewPanel.Listener, HeroIllustBook.Listener, HeroDetailView.Listener, HeroFullSceneViewBase.Listener, ArchiveSkillDesc.Listener
{
	private enum PageDirection
	{
		Prev,
		Next
	}

	private CachedHeroInfo _currentHero;

	private MonsterHeroInfo _currentMonster;

	private ArchiveType _curArchiveType;

	public static SystemLanguage SelectedLanguage;

	public string[] NORMAL_DUNGEON_LOCALE = new string[4] { "CHAPTERCLEAR_EASY", "CHAPTERCLEAR_NOMAL", "CHAPTERCLEAR_HARD", "CHAPTERCLEAR_HELL" };

	public string[] DRAGON_RAID_LOCALE = new string[2] { "CHAPTERCLEAR_NOMAL", "CHAPTERCLEAR_HARD" };

	public string[] GUILD_RAID_LOCALE = new string[4] { "CHAPTERCLEAR_EASY", "ARCHIVE_EASY_NORMAL", "CHAPTERCLEAR_HARD", "CHAPTERCLEAR_HELL" };

	private Hero3DViewPanel GetHero3DView()
	{
		HeroBook heroBook = GetInstance<HeroBook>();
		if (heroBook == null)
		{
			return null;
		}
		return heroBook.Hero3DViewPanel;
	}

	public string GetDifficultyLocale(ArchiveType ArchiveType, int localeNum)
	{
		string key = "";
		switch (ArchiveType)
		{
		case ArchiveType.Normal:
			key = (Vespa_Generic_IList_Extension_Method.IsValidIndex(NORMAL_DUNGEON_LOCALE, localeNum) ? NORMAL_DUNGEON_LOCALE[localeNum] : "");
			break;
		case ArchiveType.Raid:
			key = (Vespa_Generic_IList_Extension_Method.IsValidIndex(DRAGON_RAID_LOCALE, localeNum) ? DRAGON_RAID_LOCALE[localeNum] : "");
			break;
		case ArchiveType.GuildRaid:
			key = (Vespa_Generic_IList_Extension_Method.IsValidIndex(GUILD_RAID_LOCALE, localeNum) ? GUILD_RAID_LOCALE[localeNum] : "");
			break;
		}
		return LocalizationManager.GetStringDirect(key);
	}

	protected override void Awake()
	{
		base.Awake();
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<IllustratedBookList>("Resources/Prefab2/Window/IllustratedBook/IllustratedBookList");
		AddInstance<HeroListBook>("Resources/Prefab2/Window/IllustratedBook/HeroListBook");
		AddInstance<HeroBook>("Resources/Prefab2/Window/IllustratedBook/HeroBook");
		AddInstance<ItemListBook>("Resources/Prefab2/Window/IllustratedBook/ItemListBook");
		AddInstance<ItemBook>("Resources/Prefab2/Window/IllustratedBook/ItemBook");
		AddInstance<MonsterListBook>("Resources/Prefab2/Window/IllustratedBook/MonsterListBook");
		AddInstance<ChapterMonsterBook>("Resources/Prefab2/Window/IllustratedBook/ChapterMonsterBook");
		AddInstance<BossMonsterInfoBook>("Resources/Prefab2/Window/IllustratedBook/BossMonsterInfoBook");
		AddInstance<HeroIllustBook>("Resources/Prefab2/Window/IllustratedBook/HeroIllustBook");
		AddInstance<ArchiveSkillDesc>("Resources/Prefab2/Window/IllustratedBook/ArchiveSkillDesc");
		AddInstance<Item3DBook>("Resources/Prefab2/Window/IllustratedBook/Item3DBook");
		AddInstance<MatchBanPickCostumeRewardView>("Resources/Prefab2/Window/IllustratedBook/MatchBanPickCostumeRewardView");
		AddInstance<PetListBook>("Resources/Prefab2/Window/Pet/PetListBook");
	}

	public void OpenIllustratedBookList()
	{
		GetInstance<IllustratedBookList>().Open(this);
	}

	public void OpenHeroListBook()
	{
		GetInstance<HeroListBook>().Open(this);
	}

	public void OpenHeroBook(CachedHeroInfo heroInfo)
	{
		OpenHeroAsync(heroInfo).Commit();
	}

	private GroupTask OpenHeroAsync(CachedHeroInfo heroInfo)
	{
		CachedHeroInfo cloneHero = null;
		return Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
		{
			_currentHero = heroInfo;
			cloneHero = JsonMarshaler.Clone(heroInfo);
		}).ADDTASK(new BranchTask().If(base.TaskFactory.IsOpen<HeroBook>()).Then(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Run(delegate(HeroBook window)
		{
			window.SelectHero(cloneHero);
		})), delegate
		{
			HeroBook heroBook = GetInstance<HeroBook>();
			heroBook.SetupHero3D();
			heroBook.OnChangeSubPanel(cloneHero);
		})).Else(base.TaskFactory.Open(delegate(HeroBook window)
		{
			window.Init(this, cloneHero);
		})));
	}

	private void OpenBossMonsterInfo(MonsterHeroInfo monsterInfo, string previewBg = "")
	{
		OpenMonsterAsync(monsterInfo, previewBg).Commit();
	}

	private GroupTask OpenMonsterAsync(MonsterHeroInfo monsterInfo, string previewBg)
	{
		return Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask(), delegate
		{
			_currentMonster = monsterInfo;
		}).ADDTASK(new BranchTask().If(base.TaskFactory.IsOpen<BossMonsterInfoBook>()).Then(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Run(delegate(BossMonsterInfoBook window)
		{
			window.SelectMonster(_currentMonster);
		})), delegate
		{
			GetInstance<BossMonsterInfoBook>().RefreshInfo();
		})).Else(base.TaskFactory.Open(delegate(BossMonsterInfoBook window)
		{
			BossMonsterInfoBook.BossMonsterInfo value = default(BossMonsterInfoBook.BossMonsterInfo);
			value.ArchiveType = _curArchiveType;
			value.IListener = this;
			value.MonsterHeroInfo = _currentMonster;
			value.PreviewBg = previewBg;
			window.Init(value);
		})));
	}

	public void OpenIllustBook(int costumeIndex, List<int> costumIndexList)
	{
		HeroIllustBook window = GetInstance<HeroIllustBook>();
		window.Open(delegate
		{
			window.Init(this, _currentHero, costumeIndex, costumIndexList);
		});
	}

	public void OpenSeasonRewardIllustBook(int currentCostumeArrayIndex, List<CostumeData> seasonRewardDatas)
	{
		HeroIllustBook window = GetInstance<HeroIllustBook>();
		window.Open(delegate
		{
			window.InitSeasonReward(this, currentCostumeArrayIndex, seasonRewardDatas);
		});
	}

	public void StartRestoreHeroDetailView(Action onEnd)
	{
		VespaCoroutine.StartGlobal(CoRestoreHeroDetailView(onEnd));
	}

	private IEnumerator CoRestoreHeroDetailView(Action onEnd)
	{
		RestoreHeroDetailView();
		yield return new WaitForSeconds(1f);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public void RestoreHeroDetailView()
	{
		OpenIllustratedBookList();
		OpenHeroListBook();
		HeroPortraitComponent heroPortraitComponent = GetInstance<HeroListBook>().HeroPortraitList.Find((HeroPortraitComponent e) => e.HeroInfo.HeroIndex == HeroDetailView.HeroIndex);
		if (!(heroPortraitComponent == null))
		{
			OpenHeroBook(heroPortraitComponent.HeroInfo);
			OpenHeroDetailView(useSceneChanger: false);
		}
	}

	public void OnEnterHeroListBook()
	{
		GetInstance<HeroListBook>().Open(this);
	}

	public void OnEnterMonsterListBook()
	{
		GetInstance<MonsterListBook>().Open(this);
	}

	public void OnEnterItemListBook()
	{
		GetInstance<ItemListBook>().Open(this);
	}

	public void OnEnterPetListBook()
	{
		SceneAutoBehaviour<PetManagement>.instance.OpenPetArchiveWindow();
	}

	public void OnEnterHeroBook(CachedHeroInfo heroInfo)
	{
		OpenHeroBook(heroInfo);
	}

	public void OnPrevHero()
	{
		ChangeHeroInfo(PageDirection.Prev);
	}

	public void OnNextHero()
	{
		ChangeHeroInfo(PageDirection.Next);
	}

	public void OnShowAction()
	{
		OpenHeroDetailView();
	}

	private void OpenHeroDetailView(bool useSceneChanger = true)
	{
		if (_currentHero != null)
		{
			CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(_currentHero);
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(cachedHeroInfo.HeroIndex);
			if (heroInfo != null)
			{
				cachedHeroInfo.CostumeIndex = heroInfo.CostumeIndex;
			}
			GetInstance<HeroDetailView>().Open(cachedHeroInfo, this, HeroDetailView.EToggle.Animation, null, useSceneChanger);
		}
	}

	private void ChangeHeroInfo(PageDirection direction)
	{
		ArchiveSkillDesc archiveSkillDesc = GetInstance<ArchiveSkillDesc>();
		if (archiveSkillDesc != null && archiveSkillDesc.IsOpen())
		{
			archiveSkillDesc.Close();
		}
		HeroBook heroBook = GetInstance<HeroBook>();
		HeroListBook heroListBook = GetInstance<HeroListBook>();
		if (heroBook == null || heroListBook == null)
		{
			return;
		}
		int num = __Extension_Method_ICollection__.COUNT(heroListBook.HeroPortraitList);
		if (num == 0)
		{
			return;
		}
		CachedHeroInfo heroInfo = heroBook.HeroInfo;
		int num2 = heroListBook.HeroPortraitList.FindIndex((HeroPortraitComponent e) => e.HeroInfo.HeroIndex == heroInfo.HeroIndex);
		int num3 = 0;
		HeroPortraitComponent heroPortraitComponent = null;
		for (; num3 < num; num3++)
		{
			num2 = ((direction != PageDirection.Next) ? ((num2 <= 0) ? (num - 1) : (--num2)) : ((num2 < num - 1) ? (++num2) : 0));
			HeroPortraitComponent heroPortraitComponent2 = heroListBook.HeroPortraitList[num2];
			if (UIHelper.IsEnabledGameObjet(heroPortraitComponent2))
			{
				heroPortraitComponent = heroPortraitComponent2;
				break;
			}
		}
		if (heroPortraitComponent != null)
		{
			OpenHeroBook(heroPortraitComponent.HeroInfo);
		}
	}

	public void OnToggleChanged(HeroBook.EToggle toggle)
	{
		ArchiveSkillDesc archiveSkillDesc = GetInstance<ArchiveSkillDesc>();
		if (archiveSkillDesc != null && archiveSkillDesc.IsOpen())
		{
			archiveSkillDesc.Close();
		}
	}

	public void OnOpenArchiveSkillDesc(int skillSlotIndex, int linkedIndex)
	{
		GetInstance<ArchiveSkillDesc>().Open(this, _currentHero, skillSlotIndex, linkedIndex);
	}

	public void OnEnterBossInfo(ArchiveType ArchiveType, IMonsterPortraitProvider iProvider)
	{
		if (iProvider != null)
		{
			_curArchiveType = ArchiveType;
			OpenBossMonsterInfo(iProvider.MonsterHeroInfo, iProvider.PreviewBg);
		}
	}

	public void OnEnterChapterBook(int chapterIndex)
	{
		GetInstance<ChapterMonsterBook>().Open(chapterIndex, this);
	}

	public void OnPrevMonster()
	{
		ChangeBossMonsterInfo(PageDirection.Prev);
	}

	public void OnNextMonster()
	{
		ChangeBossMonsterInfo(PageDirection.Next);
	}

	private void ChangeBossMonsterInfo(PageDirection direction)
	{
		BossMonsterInfoBook bossMonsterInfoBook = GetInstance<BossMonsterInfoBook>();
		MonsterListBook monsterListBook = GetInstance<MonsterListBook>();
		if (bossMonsterInfoBook == null || monsterListBook == null)
		{
			return;
		}
		int num = __Extension_Method_ICollection__.COUNT(monsterListBook.PortraitComponentList);
		if (num != 0)
		{
			MonsterHeroInfo monsterInfo = bossMonsterInfoBook.MonsterInfo;
			int num2 = monsterListBook.PortraitComponentList.FindIndex((MonsterPortraitComponent e) => e.Info.HeroInfo.HeroIndex == monsterInfo.HeroInfo.HeroIndex);
			num2 = ((direction != PageDirection.Next) ? ((num2 <= 0) ? (num - 1) : (--num2)) : ((num2 < num - 1) ? (++num2) : 0));
			MonsterPortraitComponent monsterPortraitComponent = monsterListBook.PortraitComponentList[num2];
			if (monsterPortraitComponent != null)
			{
				OnEnterBossInfo(_curArchiveType, monsterPortraitComponent.Info);
			}
		}
	}

	private void OnEnterBossInfo(ArchiveType ArchiveType, MonsterHeroInfo monsterInfo)
	{
		_curArchiveType = ArchiveType;
		OpenBossMonsterInfo(monsterInfo);
	}

	public void OnToggle(BossMonsterInfoBook.EToggle toggle)
	{
	}

	public void OnEnterItemBook()
	{
	}

	public void OnToggle(Hero3DViewPanel.EToggle toggle)
	{
	}

	public void OnClickEquipIcon(ItemIconComponent caller)
	{
	}

	public void OnClickClassTagIcon(ClassTagIconComponent caller)
	{
	}

	public void OnClickDetailView()
	{
	}

	public void OnClickAutoEquip()
	{
	}

	public void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	public void OnClickSelectFlask(Hero3DViewPanel caller)
	{
	}

	public void OnClickShowUniqueWeaponOn(Hero3DViewPanel caller)
	{
	}

	public void OnClickShowUniqueWeaponOff(Hero3DViewPanel caller)
	{
	}

	public void OnClickCostume(int heroIndex)
	{
	}

	public void OnClickProfile(int heroIndex)
	{
	}

	public void OnClickPresetStorage()
	{
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		HeroDetailView.TryBattle(sender.HeroInfo);
		LobbyScene2.ListReturnState.Add(new ArchiveTryBattleReturnState());
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
		if (UIHelper.IsEnabledGameObjet(GetHero3DView()))
		{
			GetHero3DView().ActivatePreview(isActive: false);
		}
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
		if (UIHelper.IsEnabledGameObjet(GetHero3DView()))
		{
			GetHero3DView().ActivatePreview(isActive: true);
		}
	}
}
