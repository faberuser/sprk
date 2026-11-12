using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaBattlePartySetting : BaseWindow, HeroPortraitComponent.Listener, MatchBanPickSlotComponent.Listener
{
	public interface Listener
	{
		void OnClickStartBattle(int deckIndex, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] enemyHeroInfos);
	}

	[Serializable]
	public class Player
	{
		public UIGrid DeckGrid;

		public UILabel Nick;

		public UITexture Texture_GuildLogo;

		public UITexture Texture_GuildBack;

		[HideInInspector]
		public List<MatchBanPickSlotComponent> DeckSlotList = new List<MatchBanPickSlotComponent>();

		public UIFlagWidget GuildFlagWidget;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaBattlePartySetting";

	private int[] _skillIndices = new int[5] { 60000001, 60000002, 60000003, 60000004, 60000005 };

	private Listener _listener;

	private bool _isLock;

	public UIScrollGrid HeroGrid;

	private CreatureTagType _currentClassTab = CreatureTagType.End;

	public Player player1;

	public Player player2;

	public UIToggle ToggleAll;

	public UIToggle ToggleKnight;

	public UIToggle ToggleWarrior;

	public UIToggle ToggleAssassin;

	public UIToggle ToggleArcher;

	public UIToggle ToggleMechanic;

	public UIToggle ToggleWizard;

	public UIToggle TogglePriest;

	public UIButton Button_StartBattle;

	public UISimpleGrid Grid_BuffIcon;

	public MatchBanPickSlotComponent PartySlotTemplate;

	private List<SkillIconComponent> BuffComponents = new List<SkillIconComponent>();

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	private MatchBanPickSlotComponent _currentFocusedSlot;

	private GuildArenaDeckInfo _deckInfo;

	private GuildArenaMemberDeckInfo _memberInfo;

	private CachedHeroInfo[] _enemyHeroInfos;

	private int _deckIndex;

	private int[] _battledHeroIndices;

	public void Open(Listener listener, GuildArenaDeckInfo deckInfo, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] heroInfos)
	{
		base.Open();
		Init(listener, deckInfo, memberInfo, heroInfos);
	}

	private void Init(Listener listener, GuildArenaDeckInfo deckInfo, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] heroInfos)
	{
		Clear();
		_listener = listener;
		_deckInfo = deckInfo;
		_memberInfo = memberInfo;
		_enemyHeroInfos = heroInfos;
		_deckIndex = _deckInfo.DeckIndex;
		_battledHeroIndices = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetBattledHeroIndices();
		UIHelper.EnableButton(Button_StartBattle, enable: false);
		InitPlayerDeck(player1, null);
		InitPlayerDeck(player2, _enemyHeroInfos);
		SetFocusSlot(player1);
		InitHeroGrid();
		InitToggle();
		InitLabels();
		InitGuildFlags();
		InitBuffIcon();
		PlayVSEffectSound();
	}

	private void InitGuildFlags()
	{
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((myGuildInfo.Logo <= 0) ? 1 : myGuildInfo.Logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, player1.Texture_GuildLogo, guildLogoIcon);
		byte b = myGuildInfo.Back;
		if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
		{
			b = 1;
		}
		AssetTexture guildBackGround2DLong = UIHelper.GetGuildBackGround2DLong(b);
		UIHelper.ResetPlaneAssetTextureAsync(this, player1.Texture_GuildBack, guildBackGround2DLong);
		AssetTexture guildLogoIcon2 = UIHelper.GetGuildLogoIcon2((enemyGuildInfo.Logo <= 0) ? 1 : enemyGuildInfo.Logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, player2.Texture_GuildLogo, guildLogoIcon2);
		byte b2 = enemyGuildInfo.Back;
		if (b2 <= 0 || b2 > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
		{
			b2 = 1;
		}
		AssetTexture guildBackGround2DLong2 = UIHelper.GetGuildBackGround2DLong(b2);
		UIHelper.ResetPlaneAssetTextureAsync(this, player2.Texture_GuildBack, guildBackGround2DLong2);
	}

	private void InitBuffIcon()
	{
		int[] array = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("GuildArenaDefenceSkillIndices");
		if (array == null)
		{
			array = _skillIndices;
		}
		if (array.Length > _deckIndex - 1)
		{
			SkillIconComponent skillIconComponent = UIHelper.AddGridItem(Grid_BuffIcon, SkillIconComponent.PrefabAuto);
			skillIconComponent.Open(array[_deckIndex - 1]);
			BuffComponents.Add(skillIconComponent);
		}
	}

	private void PlayVSEffectSound()
	{
		VespaCoroutine.Start(coPlayVSEffectSound());
	}

	private IEnumerator coPlayVSEffectSound()
	{
		yield return new WaitForSeconds(0.2f);
		SoundStock stock = new SoundStock
		{
			Code = "UI2.Guild.GuildVSEffect"
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(BuffComponents);
	}

	public void InitLabels()
	{
		UIHelper.SetLabelText(player1.Nick, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick);
		UIHelper.SetLabelText(player2.Nick, _memberInfo.Nick);
	}

	private void ResetPlayerDeck(Player player)
	{
		player.DeckSlotList.ForEach(delegate(MatchBanPickSlotComponent e)
		{
			VespaObject.DestroyGo(e);
		});
		player.DeckSlotList.Clear();
	}

	private void InitPlayerDeck(Player player, CachedHeroInfo[] heroInfos)
	{
		player.DeckGrid.repositionNow = true;
		for (int i = 0; i < 3; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(PartySlotTemplate.gameObject);
			obj.transform.parent = player.DeckGrid.transform;
			obj.transform.localScale = PartySlotTemplate.transform.localScale;
			obj.transform.localRotation = PartySlotTemplate.transform.localRotation;
			UIHelper.EnableGameObject(obj, enable: true);
			MatchBanPickSlotComponent component = obj.GetComponent<MatchBanPickSlotComponent>();
			component.SetState(MatchBanPickSlotComponent.SlotState.None);
			component.gameObject.SetActive(value: true);
			if (heroInfos != null && heroInfos.Length > i && heroInfos[i] != null)
			{
				component.Init(null, heroInfos[i], hideInfo: false, 1L);
			}
			if (player == player2 && _deckInfo.IsBlind == 1)
			{
				component.SetBlind();
			}
			if (player == player2 && _deckInfo.IsNPC == 1)
			{
				component.SetNPC();
			}
			player.DeckSlotList.Add(component);
		}
		player.DeckGrid.Reposition();
	}

	public void InitGuildFlag()
	{
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		if (myGuildInfo != null)
		{
			player1.GuildFlagWidget.Open(myGuildInfo.Flag, myGuildInfo.Logo, myGuildInfo.Back);
			player1.GuildFlagWidget.Play();
			GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
			if (enemyGuildInfo != null)
			{
				player2.GuildFlagWidget.Open(enemyGuildInfo.Flag, enemyGuildInfo.Logo, enemyGuildInfo.Back);
				player2.GuildFlagWidget.Play();
			}
		}
	}

	public void SetFocusSlot(Player player)
	{
		UIHelper.EnableButton(Button_StartBattle, GetOccupiedSlotCount() > 0);
		foreach (MatchBanPickSlotComponent deckSlot in player.DeckSlotList)
		{
			if (deckSlot.IsEmptySlot())
			{
				deckSlot.SetState(MatchBanPickSlotComponent.SlotState.Pick);
				_currentFocusedSlot = deckSlot;
				break;
			}
		}
		foreach (MatchBanPickSlotComponent deckSlot2 in player.DeckSlotList)
		{
			if (deckSlot2 != _currentFocusedSlot)
			{
				deckSlot2.SetState(MatchBanPickSlotComponent.SlotState.None);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(player1.DeckSlotList);
		FactoryBase.ReleaseAndClearList(player2.DeckSlotList);
		UIHelper.ClearPlaneAssetTexture(player1.Texture_GuildLogo);
		UIHelper.ClearPlaneAssetTexture(player2.Texture_GuildLogo);
		UIHelper.ClearPlaneAssetTexture(player1.Texture_GuildBack);
		UIHelper.ClearPlaneAssetTexture(player2.Texture_GuildBack);
		ClearBuffIcon();
		ClearHeroGrid();
		ResetPlayerDeck(player1);
		ResetPlayerDeck(player2);
		if (player1.GuildFlagWidget != null)
		{
			player1.GuildFlagWidget.Close();
		}
		if (player2.GuildFlagWidget != null)
		{
			player2.GuildFlagWidget.Close();
		}
	}

	private void ClearHeroGrid()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		if (HeroGrid != null)
		{
			HeroGrid.ClearGridItem<HeroPortraitComponent>();
		}
	}

	public void InitHeroGrid()
	{
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true))
		{
			AddHeroSmallPortraitToGrid(heroInfo);
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	private void AddHeroSmallPortraitToGrid(CachedHeroInfo info)
	{
		if (info != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroGrid, HeroPortraitComponent.SmallPrefabAuto);
			heroPortraitComponent.Open(info, this, null, HeroPortraitComponent.EPortraitMode.Icon);
			UIHelper.EnableGameObject(heroPortraitComponent, enable: true);
			if (TypeIterator.Contains(_battledHeroIndices, info.HeroIndex))
			{
				heroPortraitComponent.SetBan(ban: true);
			}
			_heroPortraitList.Add(heroPortraitComponent);
		}
	}

	public void InitToggle()
	{
		ToggleAll.Set(state: true);
		foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
		{
			UIHelper.EnableGameObject(heroPortrait, enable: true);
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	public void OnClickClassToggle()
	{
		CreatureTagType creatureTagType = CreatureTagType.None;
		if (ToggleKnight.value)
		{
			creatureTagType = CreatureTagType.Knight;
		}
		if (ToggleWarrior.value)
		{
			creatureTagType = CreatureTagType.Warrior;
		}
		if (ToggleAssassin.value)
		{
			creatureTagType = CreatureTagType.Assassin;
		}
		if (ToggleArcher.value)
		{
			creatureTagType = CreatureTagType.Archer;
		}
		if (ToggleMechanic.value)
		{
			creatureTagType = CreatureTagType.Mechanic;
		}
		if (ToggleWizard.value)
		{
			creatureTagType = CreatureTagType.Wizard;
		}
		if (TogglePriest.value)
		{
			creatureTagType = CreatureTagType.Priest;
		}
		if (creatureTagType == _currentClassTab)
		{
			return;
		}
		_currentClassTab = creatureTagType;
		foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
		{
			bool enable = _currentClassTab == CreatureTagType.None || heroPortrait.CreatureData.TagType == _currentClassTab;
			UIHelper.EnableGameObject(heroPortrait, enable);
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	private int GetOccupiedSlotCount()
	{
		int num = 0;
		foreach (MatchBanPickSlotComponent deckSlot in player1.DeckSlotList)
		{
			if (!deckSlot.IsEmptySlot())
			{
				num++;
			}
		}
		return num;
	}

	public void OnClickStartBattle()
	{
		if (_listener == null)
		{
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (MatchBanPickSlotComponent deckSlot in player1.DeckSlotList)
		{
			list.Add(deckSlot.HeroInfo);
		}
		_listener.OnClickStartBattle(_deckIndex, _memberInfo, list.ToArray());
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait.IsBanned)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_ALREADY_BATTLE"));
		}
		else if (heroPortrait.IsSelected)
		{
			foreach (MatchBanPickSlotComponent deckSlot in player1.DeckSlotList)
			{
				if (deckSlot.HeroInfo != null && deckSlot.HeroInfo == heroPortrait.HeroInfo)
				{
					deckSlot.InitEmpty();
					heroPortrait.SetPortraitState(EHeroState.Free);
					break;
				}
			}
			foreach (HeroPortraitComponent heroPortrait2 in _heroPortraitList)
			{
				if (heroPortrait.HeroInfo.HeroIndex == heroPortrait2.HeroInfo.HeroIndex)
				{
					heroPortrait.SetPortraitState(EHeroState.Free);
				}
			}
			SetFocusSlot(player1);
		}
		else if (GetOccupiedSlotCount() == player1.DeckSlotList.Count)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_BATTLE_DECK_FULL"));
		}
		else if (_currentFocusedSlot.IsEmptySlot())
		{
			_currentFocusedSlot.Init(this, heroPortrait.HeroInfo, hideInfo: false, 0L);
			heroPortrait.SetPortraitState(EHeroState.Party);
			SetFocusSlot(player1);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void MatchBanPickSlotComponent.Listener.OnLongClick(CachedHeroInfo heroInfo, long accountId)
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(heroInfo);
	}

	void MatchBanPickSlotComponent.Listener.OnClick(CachedHeroInfo heroInfo, long accountId)
	{
		foreach (MatchBanPickSlotComponent deckSlot in player1.DeckSlotList)
		{
			if (deckSlot.HeroInfo != null && deckSlot.HeroInfo.HeroIndex == heroInfo.HeroIndex)
			{
				deckSlot.InitEmpty();
				break;
			}
		}
		foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
		{
			if (heroPortrait.HeroInfo.HeroIndex == heroInfo.HeroIndex)
			{
				heroPortrait.SetPortraitState(EHeroState.Free);
			}
		}
		SetFocusSlot(player1);
	}
}
