using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.GetGuildArenaMemberDeck;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaSetting : BaseWindow, HeroPortraitComponent.Listener, GuildArenaDeckComponent.IListener, GuildArenaMemberComponent.Listener
{
	public interface Listener
	{
		void OpenGuildArenaSettingInfo();

		void OnClickSkillSetting();

		void OnClickSaveDeck(List<int[]> heroIndices);
	}

	public enum EToggle
	{
		MyDeck,
		GuildMemberDeck
	}

	private Listener _listener;

	private EToggle _toggle = EToggle.GuildMemberDeck;

	public UIScrollGrid HeroListGrid;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public UIScrollGrid DefenceDeckListGrid;

	private List<GuildArenaDeckComponent> _defenceDeckList = new List<GuildArenaDeckComponent>();

	public UIScrollGrid GuildMemberListGrid;

	private List<GuildArenaMemberDeckComponent> _guildMemberDeckList = new List<GuildArenaMemberDeckComponent>();

	private GuildArenaDeckComponent _currentDeck;

	private GuildArenaMemberDeckComponent _currentMember;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaSetting";

	private bool IsDeckSaved = true;

	private int _deckCount;

	public UIButton Button_DefenceInfo;

	public UIButton Button_SkillSetting;

	public UIButton Button_DeckSave;

	public UIToggle Toggle_MyDeckInfo;

	public UIToggle Toggle_GuildMemberDeckInfo;

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (!Toggle_MyDeckInfo.value && !Toggle_GuildMemberDeckInfo.value)
		{
			ResetToggle();
		}
	}

	public void Open(Listener listener, GuildArenaDeckInfo[] deckInfos, Action onOpen = null)
	{
		_listener = listener;
		_deckCount = 3;
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo() != null)
		{
			int tierIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().TierIndex;
			GuildArenaTierData data = GameRoot.Instance.GuildArenaTierDataContainer.GetData(tierIndex);
			if (data != null)
			{
				_deckCount = data.DefenseDeckCount;
			}
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetSavedDeckInfos(deckInfos);
			base.Open(delegate
			{
				Init();
			});
		}
	}

	public override void Close()
	{
		if (!IsDeckSaved)
		{
			AskDeckSavePopup_Close();
			return;
		}
		if (SceneAutoBehaviour<GuildArenaManagement>.instance.IsOpened<GuildArena>())
		{
			SceneAutoBehaviour<GuildArenaManagement>.instance.GetInstance<GuildArena>().SetButtonEffect();
		}
		base.Close();
	}

	private IEnumerator coClose()
	{
		yield return new WaitForSeconds(0.5f);
		base.Close();
		yield return null;
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_defenceDeckList);
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		FactoryBase.ReleaseAndClearList(_guildMemberDeckList);
		_currentDeck = null;
		_currentMember = null;
		_toggle = EToggle.GuildMemberDeck;
		IsDeckSaved = true;
	}

	public void Init()
	{
		Clear();
		_toggle = EToggle.MyDeck;
		SetToggle(_toggle);
		RefreshTabInfoList(EToggle.MyDeck);
	}

	public void SetToggle(EToggle toggle)
	{
		Toggle_MyDeckInfo.Set(toggle == EToggle.MyDeck);
		Toggle_GuildMemberDeckInfo.Set(toggle == EToggle.GuildMemberDeck);
	}

	public void RefreshTabInfoList(EToggle toggle)
	{
		InitButton();
		InitLeftUiGrid();
		switch (toggle)
		{
		case EToggle.MyDeck:
			InitHeroPortraitList();
			InitDefenceDeckList();
			SetDefenceDecks(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos());
			SetFocusDefenceDeck();
			break;
		case EToggle.GuildMemberDeck:
			InitGuildMemberList();
			break;
		}
	}

	public void InitButton()
	{
		if (_toggle == EToggle.GuildMemberDeck)
		{
			UIHelper.EnableButton(Button_DefenceInfo, enable: false);
			UIHelper.EnableButton(Button_SkillSetting, enable: false);
			UIHelper.EnableButton(Button_DeckSave, enable: false);
			return;
		}
		GuildArenaDeckInfo[] savedDeckInfos = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos();
		bool enable = false;
		if (savedDeckInfos != null && savedDeckInfos.Length != 0)
		{
			GuildArenaDeckInfo[] array = savedDeckInfos;
			foreach (GuildArenaDeckInfo guildArenaDeckInfo in array)
			{
				if (guildArenaDeckInfo.CachedHeroInfos != null && guildArenaDeckInfo.CachedHeroInfos.Length != 0)
				{
					enable = true;
					break;
				}
			}
		}
		UIHelper.EnableButton(Button_DefenceInfo, enable: true);
		UIHelper.EnableButton(Button_SkillSetting, enable, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_DeckSave, enable: true);
	}

	public void InitLeftUiGrid()
	{
		UIHelper.EnableGameObject(HeroListGrid, _toggle == EToggle.MyDeck);
		UIHelper.EnableGameObject(GuildMemberListGrid, _toggle == EToggle.GuildMemberDeck);
	}

	public void InitHeroPortraitList()
	{
		GuildArenaDeckInfo[] savedDeckInfos = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos();
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true).ForEach(delegate(CachedHeroInfo heroInfo)
		{
			CreateHeroItem(heroInfo, savedDeckInfos);
		});
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreateHeroItem(CachedHeroInfo heroInfo, GuildArenaDeckInfo[] deckInfos)
	{
		if (heroInfo == null)
		{
			return;
		}
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
		UIHelper.EnableGameObject(heroPortraitComponent, enable: true);
		for (int i = 0; i < deckInfos.Length; i++)
		{
			if (i >= _deckCount || deckInfos[i] == null || deckInfos[i].CachedHeroInfos == null)
			{
				continue;
			}
			CachedHeroInfo[] cachedHeroInfos = deckInfos[i].CachedHeroInfos;
			foreach (CachedHeroInfo cachedHeroInfo in cachedHeroInfos)
			{
				if (heroInfo.HeroIndex == cachedHeroInfo.HeroIndex)
				{
					heroPortraitComponent.SetDeckSlot(enable: true, i + 1);
					break;
				}
			}
		}
		_heroPortraitList.Add(heroPortraitComponent);
	}

	public void InitDefenceDeckList()
	{
		FactoryBase.ReleaseAndClearList(_defenceDeckList);
		int defenseDeckCount = GameRoot.Instance.GuildArenaTierDataContainer.GetHighestTierData().DefenseDeckCount;
		for (int i = 0; i < defenseDeckCount; i++)
		{
			CreateDeckItem(i);
		}
		UIHelper.RepositionGrid(DefenceDeckListGrid);
	}

	private void CreateDeckItem(int deckIndex)
	{
		GuildArenaDeckComponent guildArenaDeckComponent = UIHelper.AddGridItem(DefenceDeckListGrid, GuildArenaDeckComponent.PrefabAuto);
		guildArenaDeckComponent.Init(deckIndex + 1, 3, this, TeamType.Left);
		_defenceDeckList.Add(guildArenaDeckComponent);
	}

	public void SetDefenceDecks(GuildArenaDeckInfo[] deckInfos = null, bool isMyDeck = true)
	{
		for (int i = 0; i < _defenceDeckList.Count; i++)
		{
			GuildArenaDeckComponent guildArenaDeckComponent = _defenceDeckList[i];
			if (i >= _deckCount)
			{
				UIHelper.EnableCollidersInChildren(guildArenaDeckComponent, isEnable: false);
				guildArenaDeckComponent.SetDisableBG();
				continue;
			}
			GuildArenaDeckInfo guildArenaDeckInfo = deckInfos[i];
			if (guildArenaDeckInfo == null || guildArenaDeckInfo.CachedHeroInfos == null)
			{
				continue;
			}
			if (guildArenaDeckInfo.HeroIndices == null || guildArenaDeckInfo.HeroIndices.Length < guildArenaDeckInfo.CachedHeroInfos.Length)
			{
				CachedHeroInfo[] cachedHeroInfos = guildArenaDeckInfo.CachedHeroInfos;
				foreach (CachedHeroInfo heroInfo in cachedHeroInfos)
				{
					HeroPortraitComponent emptySlot = guildArenaDeckComponent.GetEmptySlot();
					if (emptySlot != null)
					{
						emptySlot.ResetPortrait(heroInfo, null);
					}
				}
				continue;
			}
			for (int k = 0; k < guildArenaDeckInfo.HeroIndices.Length; k++)
			{
				int num = guildArenaDeckInfo.HeroIndices[k];
				CachedHeroInfo[] cachedHeroInfos = guildArenaDeckInfo.CachedHeroInfos;
				foreach (CachedHeroInfo cachedHeroInfo in cachedHeroInfos)
				{
					if (cachedHeroInfo.HeroIndex == num)
					{
						HeroPortraitComponent emptySlot2 = guildArenaDeckComponent.GetEmptySlot();
						if (emptySlot2 != null)
						{
							emptySlot2.ResetPortrait(cachedHeroInfo, null);
							break;
						}
					}
				}
			}
		}
	}

	public void SetFocusDefenceDeck(bool canModifyDeck = true)
	{
		if (!canModifyDeck)
		{
			for (int i = 0; i < _deckCount; i++)
			{
				_defenceDeckList[i].SetFocus(focus: false);
			}
			return;
		}
		for (int j = 0; j < _deckCount; j++)
		{
			if (_defenceDeckList[j].HasEmptySlot())
			{
				_currentDeck = _defenceDeckList[j];
				_currentDeck.SetFocus(focus: true);
				break;
			}
		}
		for (int k = 0; k < _deckCount; k++)
		{
			if (_defenceDeckList[k] != _currentDeck)
			{
				_defenceDeckList[k].SetFocus(focus: false);
			}
		}
	}

	private void InitGuildMemberList()
	{
		FactoryBase.ReleaseAndClearList(_guildMemberDeckList);
		SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaMemberDeck(delegate(NShared.GetGuildArenaMemberDeck.Response response)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.MemberDeckInfos))
			{
				GuildArenaMemberDeckInfo[] memberDeckInfos = response.MemberDeckInfos;
				Array.Sort(memberDeckInfos, SortGuildMember);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(memberDeckInfos); i++)
				{
					GuildArenaMemberDeckInfo guildArenaMemberDeckInfo = memberDeckInfos[i];
					if (guildArenaMemberDeckInfo != null)
					{
						GuildArenaMemberDeckComponent guildArenaMemberDeckComponent = UIHelper.AddGridItem(GuildMemberListGrid, GuildArenaMemberDeckComponent.PrefabAuto);
						guildArenaMemberDeckComponent.Open(this, TeamType.None, guildArenaMemberDeckInfo);
						bool flag = guildArenaMemberDeckInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
						guildArenaMemberDeckComponent.ShowSelectImage(flag);
						if (flag)
						{
							_currentMember = guildArenaMemberDeckComponent;
						}
						_guildMemberDeckList.Add(guildArenaMemberDeckComponent);
					}
				}
				UIHelper.RepositionGrid(GuildMemberListGrid);
				SetFocusGuildMemberDeck(_currentMember.MemberDeckInfo);
			}
		});
	}

	private int SortGuildMember(GuildArenaMemberDeckInfo l, GuildArenaMemberDeckInfo r)
	{
		if (l.AccountId == r.AccountId)
		{
			return 0;
		}
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		if (l.AccountId == accountId)
		{
			return -1;
		}
		if (r.AccountId == accountId)
		{
			return 1;
		}
		return -l.TeamLevel.CompareTo(r.TeamLevel);
	}

	private void SetFocusGuildMember(GuildArenaMemberDeckInfo deckInfo)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_guildMemberDeckList); i++)
		{
			GuildArenaMemberDeckComponent guildArenaMemberDeckComponent = _guildMemberDeckList[i];
			bool flag = guildArenaMemberDeckComponent.MemberDeckInfo == deckInfo;
			if (flag)
			{
				_currentMember = guildArenaMemberDeckComponent;
			}
			guildArenaMemberDeckComponent.ShowSelectImage(flag);
		}
	}

	private void SetFocusGuildMemberDeck(GuildArenaMemberDeckInfo deckInfo)
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaDeck("", 0L, 0L, deckInfo.AccountId, delegate(GuildArenaDeckInfo[] deckInfos)
		{
			InitDefenceDeckList();
			SetDefenceDecks(deckInfos, isMyDeck: false);
			SetFocusDefenceDeck(canModifyDeck: false);
		});
	}

	public void OnClickGuildArenaSettingInfo()
	{
		if (_listener != null)
		{
			_listener.OpenGuildArenaSettingInfo();
		}
	}

	public void NotApplyAndClose()
	{
		base.Close();
	}

	public void OnDeckSaved()
	{
		IsDeckSaved = true;
		CenterMessage.Open(LocalizationManager.GetStringDirect("DEFENCE_TEAM_SAVE"));
		InitButton();
	}

	public bool IsSetHero(HeroPortraitComponent heroPortrait)
	{
		for (int i = 0; i < _deckCount; i++)
		{
			if (_defenceDeckList[i].IsSetHero(heroPortrait))
			{
				return true;
			}
		}
		return false;
	}

	public void OnClickSaveDeck()
	{
		if (_listener != null)
		{
			List<int[]> list = new List<int[]>();
			for (int i = 0; i < _deckCount; i++)
			{
				int[] heroIndices = _defenceDeckList[i].GetHeroIndices();
				list.Add(heroIndices);
			}
			_listener.OnClickSaveDeck(list);
			SoundStock stock = new SoundStock
			{
				Code = "UI2.Guild.GuildGuildSaveDeck"
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
		}
	}

	public void ResetDeck()
	{
		_currentDeck = null;
		InitDefenceDeckList();
		SetDefenceDecks(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos());
		SetFocusDefenceDeck();
	}

	public void ResetToggle(bool isDefaultToggle = true)
	{
		Toggle_GuildMemberDeckInfo.value = !isDefaultToggle;
		Toggle_MyDeckInfo.value = isDefaultToggle;
	}

	public void OnClickSkillSetting()
	{
		if (_listener != null)
		{
			_listener.OnClickSkillSetting();
		}
	}

	private void ChangeTab(EToggle eToggle)
	{
		_toggle = eToggle;
		SetToggle(_toggle);
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.MyDeck;
		if (Toggle_MyDeckInfo.value)
		{
			eToggle = EToggle.MyDeck;
		}
		else if (Toggle_GuildMemberDeckInfo.value)
		{
			eToggle = EToggle.GuildMemberDeck;
		}
		if (eToggle == EToggle.GuildMemberDeck && !IsDeckSaved)
		{
			ResetToggle();
			AskDeckSavePopup_ChangeTab();
		}
		else if (_toggle != eToggle)
		{
			_toggle = eToggle;
			RefreshTabInfoList(_toggle);
		}
	}

	public void AskDeckSavePopup_Close()
	{
		if (IsDeckSaved)
		{
			return;
		}
		if (_currentDeck != null)
		{
			_currentDeck.SetFocusEffect(enable: false);
		}
		MessagePopup3Button.Open(LocalizationManager.GetStringDirect("COMMON_NOTIFICATION"), LocalizationManager.GetStringDirect("GUILD_ARENA_SETTING_NOT_APPLY"), delegate
		{
			VespaCoroutine.Start(coClose());
		}, delegate
		{
			OnClickSaveDeck();
			if (_currentDeck != null)
			{
				_currentDeck.SetFocusEffect(enable: true);
			}
		}, delegate
		{
			if (_currentDeck != null)
			{
				_currentDeck.SetFocusEffect(enable: true);
			}
		}, LocalizationManager.GetStringDirect("COMMON_EXIT"), LocalizationManager.GetStringDirect("COMMON_SAVE"));
	}

	public void AskDeckSavePopup_ChangeTab()
	{
		if (!IsDeckSaved)
		{
			if (_currentDeck != null)
			{
				_currentDeck.SetFocusEffect(enable: false);
			}
			MessagePopup3Button.Open(LocalizationManager.GetStringDirect("COMMON_NOTIFICATION"), LocalizationManager.GetStringDirect("GUILD_ARENA_SETTING_NOT_APPLY"), delegate
			{
				IsDeckSaved = true;
				ChangeTab(EToggle.GuildMemberDeck);
			}, delegate
			{
				OnClickSaveDeck();
			}, null, LocalizationManager.GetStringDirect("COMMON_EXIT"), LocalizationManager.GetStringDirect("COMMON_SAVE"));
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (IsSetHero(heroPortrait))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_ALREADY_DEFENCE_SETTING"));
			return;
		}
		if (_currentDeck != null)
		{
			GuildArenaDeckComponent guildArenaDeckComponent = null;
			for (int i = 0; i < _deckCount; i++)
			{
				if (!_defenceDeckList[i].HasEmptySlot())
				{
					guildArenaDeckComponent = _defenceDeckList[i];
					break;
				}
			}
			HeroPortraitComponent emptySlot = _currentDeck.GetEmptySlot();
			if (emptySlot != null)
			{
				emptySlot.ResetPortrait(heroPortrait.HeroInfo, null);
				heroPortrait.SetDeckSlot(enable: true, _currentDeck.DeckIndex);
				if (!_currentDeck.HasEmptySlot())
				{
					SetFocusDefenceDeck();
				}
				IsDeckSaved = false;
				SoundStock stock = new SoundStock
				{
					Code = "UI2.Guild.GuildHeroPick"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
				return;
			}
			if (guildArenaDeckComponent != null && !_currentDeck.HasEmptySlot())
			{
				SetFocusDefenceDeck();
				emptySlot = _currentDeck.GetEmptySlot();
				if (emptySlot != null)
				{
					emptySlot.ResetPortrait(heroPortrait.HeroInfo, null);
					heroPortrait.SetDeckSlot(enable: true, _currentDeck.DeckIndex);
					SoundStock stock2 = new SoundStock
					{
						Code = "UI2.Guild.GuildHeroPick"
					};
					SingletonAuto<SoundContainer>.instance.PlayOnce(stock2, 1f);
					IsDeckSaved = false;
					return;
				}
			}
		}
		CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_ARENA_DEFENCE_FULL"));
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void GuildArenaDeckComponent.IListener.OnClickDeckHeroPortrait(HeroPortraitComponent component)
	{
		if (component.IsEmptySlot() || _toggle != EToggle.MyDeck)
		{
			return;
		}
		foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
		{
			if (heroPortrait.HeroInfo.HeroIndex != component.HeroInfo.HeroIndex)
			{
				continue;
			}
			heroPortrait.SetDeckSlot(enable: false);
			component.ResetPortrait(null, null);
			IsDeckSaved = false;
			for (int i = 0; i < _deckCount; i++)
			{
				if (_defenceDeckList[i].HasComponent(component))
				{
					_currentDeck = _defenceDeckList[i];
				}
				_defenceDeckList[i].SetFocus(_defenceDeckList[i].HasComponent(component));
			}
			SoundStock stock = new SoundStock
			{
				Code = "UI2.Guild.GuildHeroPick"
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
			break;
		}
	}

	void GuildArenaDeckComponent.IListener.OnClickStartBattle(GuildArenaDeckInfo deckInfo, CachedHeroInfo[] heroInfos)
	{
	}

	void GuildArenaDeckComponent.IListener.OnClickDeck(GuildArenaDeckComponent component)
	{
		for (int i = 0; i < _deckCount; i++)
		{
			_defenceDeckList[i].SetFocus(_defenceDeckList[i] == component);
			if (_defenceDeckList[i] == component)
			{
				_currentDeck = _defenceDeckList[i];
			}
		}
	}

	void GuildArenaDeckComponent.IListener.OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName)
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArenaDeckDetailPopup(heroInfos, titleName);
	}

	void GuildArenaMemberComponent.Listener.OnClickDeckAttack(GuildArenaMemberDeckInfo deckInfo)
	{
	}

	void GuildArenaMemberComponent.Listener.OnClickDeckInfo(GuildArenaMemberDeckInfo deckInfo)
	{
		if (!(_currentMember != null) || _currentMember.MemberDeckInfo != deckInfo)
		{
			SetFocusGuildMember(deckInfo);
			SetFocusGuildMemberDeck(deckInfo);
		}
	}
}
