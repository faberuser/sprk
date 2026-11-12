using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EclipseBattleReady : BaseWindow, HeroPortraitComponent.Listener, EclipseDeckComponent.IListener
{
	public interface Listener
	{
		void OnClickStartBattle();

		void OnClickGiveUp();

		void OnClickGetReward();

		void OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName);
	}

	private Listener _listener;

	public UIAnimationScrollGrid DefenceDeckListGrid;

	private List<EclipseDeckComponent> _teamList = new List<EclipseDeckComponent>();

	private EclipseDeckComponent _currentDeck;

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseBattleReady";

	private bool IsDeckSaved = true;

	public UIButton Button_GiveUp;

	public UIButton Button_BattleStart;

	public UIButton Button_GetReward;

	public EclipseInfo _eclipseInfo;

	private bool _isExpire;

	private int _maxDeckCount;

	private float _lastRefreshInfoTime;

	private bool IsShowGetReward
	{
		get
		{
			if (_eclipseInfo == null)
			{
				return false;
			}
			bool num = _eclipseInfo.DeckIndex == _eclipseInfo.LastDeckIndex;
			bool flag = EclipseMain.GetRemainTime(_eclipseInfo.ExpireTime) <= 0;
			return num || flag;
		}
	}

	private int MaxDeckCount
	{
		get
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return _maxDeckCount;
			}
			EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
			if (savedDeckInfos == null)
			{
				return _maxDeckCount;
			}
			if (savedDeckInfos.Length <= _maxDeckCount)
			{
				return savedDeckInfos.Length;
			}
			return _maxDeckCount;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Open(Listener listener, GuildArenaDeckInfo[] deckInfos, int maxDeckCount, Action onOpen = null)
	{
		_listener = listener;
		_maxDeckCount = maxDeckCount;
		ResetInfo();
		base.Open(delegate
		{
			Init();
		});
	}

	public override void Close()
	{
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
		FactoryBase.ReleaseAndClearList(_teamList);
		_currentDeck = null;
		IsDeckSaved = true;
	}

	public void Init()
	{
		Clear();
		SetButtons();
		InitDeckList();
		SetDecks();
		SetDefeatDecks();
		FocusDeck();
	}

	private void FocusDeck()
	{
		if (__Extension_Method_ICollection__.COUNT(_teamList) == 0)
		{
			return;
		}
		bool flag = _eclipseInfo.DeckIndex == _eclipseInfo.LastDeckIndex;
		int deckIndex = _eclipseInfo.DeckIndex;
		for (int num = __Extension_Method_ICollection__.COUNT(_teamList) - 1; num >= 0; num--)
		{
			EclipseDeckComponent eclipseDeckComponent = _teamList[num];
			if (eclipseDeckComponent.DeckIndex < deckIndex || (flag && eclipseDeckComponent.DeckIndex == deckIndex))
			{
				DefenceDeckListGrid.ScrollTo(eclipseDeckComponent, isRestrict: true);
				break;
			}
		}
	}

	private void SetButtons()
	{
		_ = _eclipseInfo.DeckIndex;
		_ = _eclipseInfo.LastDeckIndex;
		EclipseMain.GetRemainTime(_eclipseInfo.ExpireTime);
		bool isShowGetReward = IsShowGetReward;
		UIHelper.EnableGameObject(Button_GiveUp, !isShowGetReward);
		UIHelper.EnableGameObject(Button_BattleStart, !isShowGetReward);
		UIHelper.EnableGameObject(Button_GetReward, isShowGetReward);
	}

	private void InitDeckList()
	{
		FactoryBase.ReleaseAndClearList(_teamList);
		int maxDeckCount = MaxDeckCount;
		for (int i = 0; i < maxDeckCount; i++)
		{
			CreateDeckItem(i);
		}
		UIHelper.RepositionGrid(DefenceDeckListGrid);
	}

	private void CreateDeckItem(int deckIndex)
	{
		EclipseDeckComponent eclipseDeckComponent = UIHelper.AddGridItem(DefenceDeckListGrid, EclipseDeckComponent.PrefabAuto);
		eclipseDeckComponent.Init(this, deckIndex + 1, 4, enableDetailView: true);
		_teamList.Add(eclipseDeckComponent);
	}

	private void SetDecks()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		if (savedDeckInfos == null)
		{
			return;
		}
		int maxDeckCount = MaxDeckCount;
		for (int i = 0; i < maxDeckCount; i++)
		{
			if (savedDeckInfos.Length <= i)
			{
				continue;
			}
			EclipseDeckResult eclipseDeckResult = savedDeckInfos[i];
			if (eclipseDeckResult == null || String_Extension_Method.IsNullOrEmpty(eclipseDeckResult.HeroIndices))
			{
				continue;
			}
			string[] array = eclipseDeckResult.HeroIndices.Split(',');
			foreach (string text in array)
			{
				if (_teamList.Count <= i)
				{
					continue;
				}
				EclipseDeckComponent eclipseDeckComponent = _teamList[i];
				if (eclipseDeckComponent == null)
				{
					continue;
				}
				HeroPortraitComponent emptySlot = eclipseDeckComponent.GetEmptySlot();
				if (emptySlot == null)
				{
					continue;
				}
				CachedHeroInfo cachedHeroInfo = null;
				CachedHeroInfo[] cachedHeroInfos = eclipseDeckResult.CachedHeroInfos;
				foreach (CachedHeroInfo cachedHeroInfo2 in cachedHeroInfos)
				{
					if (text.Equals(cachedHeroInfo2.HeroIndex.ToString()))
					{
						cachedHeroInfo = cachedHeroInfo2;
					}
				}
				if (cachedHeroInfo != null)
				{
					emptySlot.ResetPortrait(cachedHeroInfo, null);
				}
			}
		}
	}

	private void SetDefeatDecks()
	{
		bool flag = _eclipseInfo.DeckIndex == _eclipseInfo.LastDeckIndex;
		int deckIndex = _eclipseInfo.DeckIndex;
		foreach (EclipseDeckComponent team in _teamList)
		{
			if (!team.IsEmptyDeck() && (team.DeckIndex < deckIndex || (flag && team.DeckIndex == deckIndex)))
			{
				team.SetDefeatDeck();
			}
		}
	}

	public void OnDeckSaved()
	{
		IsDeckSaved = true;
		CenterMessage.Open(LocalizationManager.GetStringDirect("DEFENCE_TEAM_SAVE"));
	}

	public bool IsSetHero(HeroPortraitComponent heroPortrait)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_teamList); i++)
		{
			if (_teamList[i].IsSetHero(heroPortrait))
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (RefreshTime())
		{
			UpdateRemainTime();
		}
	}

	private bool RefreshTime()
	{
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		if (_eclipseInfo == null)
		{
			return false;
		}
		if (Time.time - _lastRefreshInfoTime < 1f)
		{
			return false;
		}
		_lastRefreshInfoTime = Time.time;
		return true;
	}

	private void UpdateRemainTime()
	{
		int remainTime = GetRemainTime();
		if (_eclipseInfo != null && _eclipseInfo.IsPlayEclipse && remainTime == 0 && !_isExpire)
		{
			_isExpire = true;
			SetButtons();
		}
	}

	private int GetRemainTime()
	{
		return EclipseMain.GetRemainTime(_eclipseInfo);
	}

	public void ResetInfo()
	{
		_eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
	}

	public void ResetDeck()
	{
		_currentDeck = null;
		InitDeckList();
		SetDecks();
	}

	public void OnClickBattleStart()
	{
		if (_listener != null)
		{
			_listener.OnClickStartBattle();
		}
	}

	public void OnClickGiveUp()
	{
		if (_listener != null)
		{
			_listener.OnClickGiveUp();
		}
	}

	public void OnClickGetReward()
	{
		if (_listener != null)
		{
			_listener.OnClickGetReward();
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
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

	void EclipseDeckComponent.IListener.OnClickStartBattle(GuildArenaDeckInfo deckInfo, CachedHeroInfo[] heroInfos)
	{
	}

	void EclipseDeckComponent.IListener.OnClickDeck(EclipseDeckComponent component)
	{
	}

	void EclipseDeckComponent.IListener.OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName)
	{
		if (_listener != null)
		{
			_listener.OnClickDeckDetailInfo(heroInfos, titleName);
		}
	}

	void EclipseDeckComponent.IListener.OnClickDeckHeroPortrait(HeroPortraitComponent component)
	{
	}
}
