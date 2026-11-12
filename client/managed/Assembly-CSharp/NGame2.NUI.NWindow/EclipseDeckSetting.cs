using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EclipseDeckSetting : BaseWindow, HeroPortraitComponent.Listener, EclipseDeckComponent.IListener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver
{
	public interface Listener : EclipseMain.ReturnStateListener
	{
		void OnClickStartBattle(EclipseDeckResult[] deckInfos);
	}

	private int componentCount = 1;

	private Listener _listener;

	public UIScrollGrid HeroListGrid;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public UIAnimationScrollGrid DefenceDeckListGrid;

	private List<EclipseDeckComponent> _defenceDeckList = new List<EclipseDeckComponent>();

	private EclipseDeckComponent _currentDeck;

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseDeckSetting";

	public const int MaxSlotCount = 4;

	private bool IsDeckSaved = true;

	private int _deckCount;

	public UIButton Button_SkillSetting;

	public UIButton Button_DeckGoUp;

	public UIButton Button_DeckGoDown;

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Open(Listener listener, int maxDeckCount, GuildArenaDeckInfo[] deckInfos, Action onOpen = null)
	{
		_listener = listener;
		_deckCount = maxDeckCount;
		base.Open(delegate
		{
			Init();
		});
	}

	public void OpenTutorial(Listener listener, int maxDeckCount)
	{
		_listener = listener;
		_deckCount = maxDeckCount;
		if (_listener != null)
		{
			_listener.SetHideRoot(enable: false);
		}
		base.Open(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.ClearSavedDeckInfos();
			Clear();
			InitHeroPortraitList();
			InitDefenceDeckList();
			SetDecks();
			SetFocusDeck();
		});
	}

	public override void Close()
	{
		base.Close();
		if (_listener != null)
		{
			_listener.SetHideRoot(enable: true);
		}
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
		_currentDeck = null;
		IsDeckSaved = true;
	}

	public void Init()
	{
		if (_listener != null)
		{
			_listener.SetHideRoot(enable: false);
		}
		float y = DefenceDeckListGrid.GetComponent<UIWidget>().localSize.y;
		float cellHeight = DefenceDeckListGrid.Grid.cellHeight;
		componentCount = (int)(y / cellHeight);
		HeroListGrid.ClearGridItem<HeroPortraitComponent>();
		Clear();
		InitHeroPortraitList();
		InitDefenceDeckList();
		SetDecks();
		SetFocusDeck();
	}

	public void InitHeroPortraitList()
	{
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true).ForEach(delegate(CachedHeroInfo heroInfo)
		{
			CreateHeroItem(heroInfo, savedDeckInfos);
		});
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreateHeroItem(CachedHeroInfo heroInfo, EclipseDeckResult[] deckInfos)
	{
		if (heroInfo == null)
		{
			return;
		}
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
		UIHelper.EnableGameObject(heroPortraitComponent, enable: true);
		if (deckInfos == null)
		{
			return;
		}
		for (int i = 0; i < deckInfos.Length && i < _deckCount; i++)
		{
			EclipseDeckResult eclipseDeckResult = deckInfos[i];
			if (eclipseDeckResult == null || String_Extension_Method.IsNullOrEmpty(eclipseDeckResult.HeroIndices))
			{
				continue;
			}
			string[] array = eclipseDeckResult.HeroIndices.Split(',');
			foreach (string value in array)
			{
				if (heroInfo.HeroIndex.ToString().Equals(value))
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
		for (int i = 0; i < _deckCount; i++)
		{
			CreateDeckItem(i);
		}
		UIHelper.RepositionGrid(DefenceDeckListGrid);
	}

	private void CreateDeckItem(int deckIndex)
	{
		EclipseDeckComponent eclipseDeckComponent = UIHelper.AddGridItem(DefenceDeckListGrid, EclipseDeckComponent.PrefabAuto);
		eclipseDeckComponent.Init(this, deckIndex + 1, 4, enableDetailView: true);
		_defenceDeckList.Add(eclipseDeckComponent);
	}

	public void SetDecks()
	{
		EclipseDeckResult[] savedDeckInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetSavedDeckInfos();
		if (savedDeckInfos == null)
		{
			return;
		}
		for (int i = 0; i < savedDeckInfos.Count(); i++)
		{
			if (i < _deckCount)
			{
				if (savedDeckInfos[i] == null || String_Extension_Method.IsNullOrEmpty(savedDeckInfos[i].HeroIndices))
				{
					continue;
				}
				string[] array = savedDeckInfos[i].HeroIndices.Split(',');
				foreach (string s in array)
				{
					HeroPortraitComponent emptySlot = _defenceDeckList[i].GetEmptySlot();
					if (!(emptySlot != null))
					{
						continue;
					}
					int result = 0;
					if (int.TryParse(s, out result) && result != 0)
					{
						CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(result);
						if (heroInfo != null)
						{
							emptySlot.ResetPortrait(heroInfo, null);
						}
					}
				}
			}
			else
			{
				UIHelper.EnableCollidersInChildren(_defenceDeckList[i], isEnable: false);
			}
		}
	}

	public void SetFocusDeck()
	{
		int num = 1;
		if (_currentDeck != null)
		{
			num = _currentDeck.DeckIndex;
		}
		for (int i = 0; i < _deckCount; i++)
		{
			if (!_defenceDeckList[i].HasEmptySlot())
			{
				continue;
			}
			bool flag = _defenceDeckList[i].DeckIndex > num;
			_currentDeck = _defenceDeckList[i];
			_currentDeck.SetFocus(focus: true);
			if (i >= componentCount || !flag)
			{
				if (i > _deckCount - componentCount && flag)
				{
					UIHelper.MoveGridPositionToSelectedItem(DefenceDeckListGrid.Grid, _currentDeck.transform.localPosition, -1);
					break;
				}
				EclipseDeckComponent comp = _defenceDeckList[Mathf.Clamp(i - 1, 0, __Extension_Method_ICollection__.COUNT(_defenceDeckList))];
				DefenceDeckListGrid.ScrollTo(comp);
			}
			break;
		}
		for (int j = 0; j < _deckCount; j++)
		{
			if (_defenceDeckList[j] != _currentDeck)
			{
				_defenceDeckList[j].SetFocus(focus: false);
			}
		}
		SetUpDownButtonEnable();
	}

	public void OnDeckSaved()
	{
		IsDeckSaved = true;
		CenterMessage.Open(LocalizationManager.GetStringDirect("DEFENCE_TEAM_SAVE"));
	}

	public int IsSetHero(HeroPortraitComponent heroPortrait)
	{
		for (int i = 0; i < _deckCount; i++)
		{
			if (_defenceDeckList[i].IsSetHero(heroPortrait))
			{
				return i + 1;
			}
		}
		return -1;
	}

	public void ResetDeck()
	{
		_currentDeck = null;
		InitDefenceDeckList();
		SetDecks();
		SetFocusDeck();
	}

	public void OnClickManageHero()
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		if (heroInfoList != null && heroInfoList.Count > 0)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(heroInfoList[0]);
		}
	}

	public void OnClickBattleStart()
	{
		List<EclipseDeckResult> list = new List<EclipseDeckResult>();
		EclipseDeckComponent eclipseDeckComponent = null;
		for (int num = _defenceDeckList.Count - 1; num > -1; num--)
		{
			if (!_defenceDeckList[num].IsEmptyDeck())
			{
				eclipseDeckComponent = _defenceDeckList[num];
				break;
			}
		}
		if (eclipseDeckComponent != null)
		{
			int deckIndex = eclipseDeckComponent.DeckIndex;
			int num2 = 1;
			for (int i = 0; i < deckIndex; i++)
			{
				if (!_defenceDeckList[i].IsEmptyDeck())
				{
					EclipseDeckResult eclipseDeckResult = new EclipseDeckResult();
					eclipseDeckResult.DeckIndex = num2;
					string[] value = (from p in _defenceDeckList[i].GetHeroIndices()
						select p.ToString()).ToArray();
					eclipseDeckResult.HeroIndices = string.Join(",", value);
					list.Add(eclipseDeckResult);
					num2++;
				}
			}
		}
		if (_listener != null && list.Count > 0)
		{
			_listener.OnClickStartBattle(list.ToArray());
		}
		else
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("NO_TEAM_HEROS"));
		}
	}

	public void OnClickGoUp()
	{
		SwapDeck(goup: true);
	}

	public void OnClickGoDown()
	{
		SwapDeck(goup: false);
	}

	private void SwapDeck(bool goup)
	{
		if (_currentDeck != null)
		{
			int deckIndex = _currentDeck.DeckIndex;
			if ((goup && deckIndex <= 1) || _currentDeck.GetEmptySlotCount() == 4)
			{
				return;
			}
			int num = deckIndex - 1;
			if (!goup)
			{
				num = deckIndex + 1;
			}
			EclipseDeckComponent targetDeck = null;
			bool flag = true;
			for (int i = 0; i < _deckCount; i++)
			{
				if (_defenceDeckList[i].DeckIndex == num)
				{
					targetDeck = _defenceDeckList[i];
					if (targetDeck.GetEmptySlotCount() != 4)
					{
						flag = false;
						break;
					}
				}
				else if (_defenceDeckList[i].DeckIndex > num && _defenceDeckList[i].GetEmptySlotCount() != 4)
				{
					flag = false;
					break;
				}
			}
			if (targetDeck != null)
			{
				if (!goup && flag)
				{
					return;
				}
				List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
				List<CachedHeroInfo> list = new List<CachedHeroInfo>();
				List<CachedHeroInfo> list2 = new List<CachedHeroInfo>();
				int[] heroIndices = _currentDeck.GetHeroIndices();
				foreach (int num2 in heroIndices)
				{
					foreach (CachedHeroInfo item in heroInfoList)
					{
						if (item.HeroIndex == num2)
						{
							list.Add(item);
							break;
						}
					}
				}
				heroIndices = targetDeck.GetHeroIndices();
				foreach (int num3 in heroIndices)
				{
					foreach (CachedHeroInfo item2 in heroInfoList)
					{
						if (item2.HeroIndex == num3)
						{
							list2.Add(item2);
							break;
						}
					}
				}
				ClearDeckSlot(_currentDeck);
				ClearDeckSlot(targetDeck);
				SetHeroDeckSlot(_currentDeck, list2);
				SetHeroDeckSlot(targetDeck, list);
				for (int k = 0; k < _deckCount; k++)
				{
					if (_defenceDeckList[k].DeckIndex == targetDeck.DeckIndex)
					{
						_currentDeck = _defenceDeckList[k];
					}
					_defenceDeckList[k].SetFocus(_defenceDeckList[k].DeckIndex == targetDeck.DeckIndex);
				}
				SoundStock stock = new SoundStock
				{
					Code = "UI2.Guild.GuildHeroPick"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
				UIHelper.CheckOutOfGridSightItem(DefenceDeckListGrid.Grid, targetDeck.transform.localPosition, delegate
				{
					UIHelper.MoveGridPositionToSelectedItem(DefenceDeckListGrid.Grid, targetDeck.transform.localPosition, 1);
				}, delegate
				{
					UIHelper.MoveGridPositionToSelectedItem(DefenceDeckListGrid.Grid, targetDeck.transform.localPosition, -1);
				});
			}
		}
		SetUpDownButtonEnable();
	}

	private void SetUpDownButtonEnable()
	{
		bool enable = true;
		bool enable2 = true;
		if (_currentDeck != null)
		{
			int deckIndex = _currentDeck.DeckIndex;
			if (deckIndex == 1)
			{
				enable = false;
			}
			else if (deckIndex == _deckCount)
			{
				enable2 = false;
			}
			if (_currentDeck.GetEmptySlotCount() == 4)
			{
				enable = false;
				enable2 = false;
			}
			int num = deckIndex + 1;
			bool flag = true;
			for (int i = 0; i < _deckCount; i++)
			{
				if (_defenceDeckList[i].DeckIndex == num)
				{
					if (_defenceDeckList[i].GetEmptySlotCount() != 4)
					{
						flag = false;
						break;
					}
				}
				else if (_defenceDeckList[i].DeckIndex > num && _defenceDeckList[i].GetEmptySlotCount() != 4)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				enable2 = false;
			}
		}
		else
		{
			enable = false;
			enable2 = false;
		}
		UIHelper.EnableButton(Button_DeckGoUp, enable);
		UIHelper.EnableButton(Button_DeckGoDown, enable2);
	}

	private void SetHeroDeckSlot(EclipseDeckComponent component, List<CachedHeroInfo> heroInfos)
	{
		foreach (CachedHeroInfo heroInfo in heroInfos)
		{
			HeroPortraitComponent emptySlot = component.GetEmptySlot();
			if (!(emptySlot != null))
			{
				continue;
			}
			emptySlot.ResetPortrait(heroInfo, null);
			foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
			{
				if (heroPortrait.HeroInfo != null && heroPortrait.HeroInfo.HeroIndex == heroInfo.HeroIndex)
				{
					heroPortrait.SetDeckSlot(enable: true, component.DeckIndex);
					break;
				}
			}
		}
	}

	private void ClearDeckSlot(EclipseDeckComponent component)
	{
		if (component == null)
		{
			return;
		}
		List<HeroPortraitComponent> heroPortraitComponents = component.GetHeroPortraitComponents();
		if (heroPortraitComponents == null && __Extension_Method_ICollection__.COUNT(heroPortraitComponents) <= 0)
		{
			return;
		}
		foreach (HeroPortraitComponent item in heroPortraitComponents)
		{
			if (item == null)
			{
				continue;
			}
			foreach (HeroPortraitComponent heroPortrait in _heroPortraitList)
			{
				if (!(heroPortrait == null) && heroPortrait.HeroInfo != null && item.HeroInfo != null && heroPortrait.HeroInfo.HeroIndex == item.HeroInfo.HeroIndex)
				{
					heroPortrait.SetDeckSlot(enable: false);
					item.ResetPortrait(null, null);
					break;
				}
			}
		}
		component.CreateHeroIcons(4);
		IsDeckSaved = false;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		int num = IsSetHero(heroPortrait);
		if (num > 0)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("ECLIPSE_HERO_ALREADY_SET_TEAM_CENTER_MSG", num));
			return;
		}
		if (_currentDeck != null)
		{
			EclipseDeckComponent eclipseDeckComponent = null;
			for (int i = 0; i < _deckCount; i++)
			{
				if (!_defenceDeckList[i].HasEmptySlot())
				{
					eclipseDeckComponent = _defenceDeckList[i];
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
					SetFocusDeck();
				}
				IsDeckSaved = false;
				SoundStock stock = new SoundStock
				{
					Code = "UI2.Guild.GuildHeroPick"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
				SetUpDownButtonEnable();
				return;
			}
			if (eclipseDeckComponent != null && !_currentDeck.HasEmptySlot())
			{
				SetFocusDeck();
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
					SetUpDownButtonEnable();
					return;
				}
			}
		}
		CenterMessage.Open(LocalizationManager.GetStringDirect("ECLIPSE_TEAM_SLOT_FULL"));
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
		for (int i = 0; i < _deckCount; i++)
		{
			_defenceDeckList[i].SetFocus(_defenceDeckList[i] == component);
			if (_defenceDeckList[i] == component)
			{
				_currentDeck = _defenceDeckList[i];
			}
		}
		SetUpDownButtonEnable();
	}

	void EclipseDeckComponent.IListener.OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName)
	{
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance)
		{
			SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<EclipseDeckDetailPopup>().Open(null, heroInfos, titleName, LocalizationManager.GetStringDirect("ECLIPSE_TEAM_HERO_EQUIP_POPUP_DESC"));
		}
	}

	void EclipseDeckComponent.IListener.OnClickDeckHeroPortrait(HeroPortraitComponent component)
	{
		if (component.IsEmptySlot())
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
		SetUpDownButtonEnable();
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		HeroPortraitComponent heroPortraitComponent = _heroPortraitList.Find((HeroPortraitComponent comp) => comp.HeroInfo.HeroIndex == changedInfo.HeroIndex);
		if (heroPortraitComponent == null)
		{
			return;
		}
		HeroPortraitComponent heroPortraitComponent2 = null;
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_defenceDeckList); num++)
		{
			EclipseDeckComponent eclipseDeckComponent = _defenceDeckList[num];
			if (!(eclipseDeckComponent == null) && !eclipseDeckComponent.IsEmptyDeck())
			{
				HeroPortraitComponent heroPortraitComponent3 = eclipseDeckComponent.GetheroPortraitComponent(changedInfo.HeroIndex);
				if (heroPortraitComponent3 != null)
				{
					heroPortraitComponent2 = heroPortraitComponent3;
					break;
				}
			}
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
		{
			heroPortraitComponent.SetLevel(show: true);
			if (heroPortraitComponent2 != null)
			{
				heroPortraitComponent2.SetLevel(show: true);
			}
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			heroPortraitComponent.SetHeroImage();
			if (heroPortraitComponent2 != null)
			{
				heroPortraitComponent2.SetHeroImage();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
