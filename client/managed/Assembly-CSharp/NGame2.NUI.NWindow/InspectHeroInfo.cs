using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NShared;
using NShared.ApplyHeroTranscendSkillPage;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InspectHeroInfo : BaseWindow, ItemIconComponent.Listener, HeroRunehengeIcon.IDataProvider
{
	[Serializable]
	public class EquipSlotInfo
	{
		public int EquipSlotIndex;

		public Transform Root;

		public Transform Sprite_Lock;
	}

	public class InspectOption
	{
		public bool ShowEquipSlot = true;

		public bool ShowHeroStar = true;

		public bool ShowHeroLevel = true;

		public bool ShowPresetButton = true;

		public string ConfirmText;
	}

	[Serializable]
	public class PresetButton
	{
		public UIButton Button;

		public UISprite ButtonSprite;

		public UISprite Select;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/InspectHeroInfo";

	public Transform HeroRoot;

	public Transform EquipRoot;

	public EquipSlotInfo[] EquipSlotInfos;

	public UILabel ConfirmButtonLabel;

	public UITable SkillGroupTable;

	public List<PresetButton> PresetButtonList;

	public HeroRunehengeIcon RunehengeIcon;

	public UIWidget PresetButtonRoot;

	public UIWidget SkillPageRoot;

	public UIWidget WidgetRuneTooltip;

	private CachedHeroInfo _cachedHeroInfo;

	private InspectOption _option;

	private Action _onConfirm;

	private HeroPortraitComponent _heroPortraitComponent;

	private List<Component> _equipItemIconComponentList = new List<Component>();

	private int CurrentSelectPage = 1;

	private int MaxPage = 3;

	private RunehengeTooltipTextInfo _runeTooltipInfo;

	private List<HeroTranscendSkillGroupComponent> _ListTranscendSkillGroup = new List<HeroTranscendSkillGroupComponent>();

	public static void Open(CachedHeroInfo cachedHeroInfo, InspectOption option, Action onConfirm)
	{
		WindowPrefab<InspectHeroInfo> windowPrefab = new WindowPrefab<InspectHeroInfo>("Resources/Prefab2/Window/Hero/InspectHeroInfo", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(cachedHeroInfo, option, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(CachedHeroInfo cachedHeroInfo, InspectOption option, Action onConfirm)
	{
		base.Open();
		_cachedHeroInfo = cachedHeroInfo;
		_option = option;
		_onConfirm = onConfirm;
		MaxPage = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendHeroSkillPage");
		SetupButton();
		SetupHeroPortrait();
		SetupEquipItemIcon();
		SetupPreset();
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Open(this);
		}
		_runeTooltipInfo = new RunehengeTooltipTextInfo();
		_runeTooltipInfo.HeroInfo = _cachedHeroInfo;
		_runeTooltipInfo.RunePage = _cachedHeroInfo.ApplyRunePage;
	}

	public override void Close()
	{
		base.Close();
		if (_option.ShowPresetButton)
		{
			SendPrevSelectedTranscendPage();
		}
		HideRunetooltip();
	}

	public override void Reset()
	{
		base.Reset();
		ClearHeroPortrait();
		ClearEquipItemIcon();
		ClearGroupList();
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Close();
		}
		_cachedHeroInfo = null;
		_option = null;
		_onConfirm = null;
	}

	public void SendPrevSelectedTranscendPage()
	{
		if (_cachedHeroInfo != null && _cachedHeroInfo.SkillPage != CurrentSelectPage)
		{
			SelectTranscendPreset(_cachedHeroInfo.HeroIndex, CurrentSelectPage);
		}
	}

	private static void SelectTranscendPreset(int heroIndex, int page)
	{
		NShared.ApplyHeroTranscendSkillPage.Request request = new NShared.ApplyHeroTranscendSkillPage.Request
		{
			HeroIndex = heroIndex,
			PageIndex = page
		};
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(request.HeroIndex);
		int oldPage = heroInfo.SkillPage;
		heroInfo.SetTranscendSkillPage(page);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.ApplyHeroTranscendSkillPage.Response response)
		{
			if (response == null)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				heroInfo.SetTranscendSkillPage(oldPage);
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SelectTranscendSkillPage(request.HeroIndex, page);
			}
		});
	}

	private void ClearHeroPortrait()
	{
		FactoryBase.SafeReleaseComponent(ref _heroPortraitComponent);
	}

	private void ClearEquipItemIcon()
	{
		FactoryBase.ReleaseAndClearList(_equipItemIconComponentList);
	}

	private void SetupButton()
	{
		UIHelper.SetLabelText(ConfirmButtonLabel, _option.ConfirmText);
	}

	private void SetupPreset()
	{
		UIHelper.EnableGameObject(SkillPageRoot, enable: false);
		UIHelper.EnableGameObject(PresetButtonRoot, _option.ShowPresetButton);
		if (_option.ShowPresetButton)
		{
			ChangePage(_cachedHeroInfo.SkillPage);
		}
	}

	private void SetupHeroPortrait()
	{
		ClearHeroPortrait();
		if (_cachedHeroInfo != null)
		{
			_heroPortraitComponent = HeroPortraitComponent.ShortPrefabAuto.Factory.Create(HeroRoot);
			_heroPortraitComponent.Open(_cachedHeroInfo, null);
			_heroPortraitComponent.ShowStar(_option.ShowHeroStar);
			_heroPortraitComponent.SetLevel(_option.ShowHeroLevel);
			_heroPortraitComponent.ShowCreatureName();
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_heroPortraitComponent.transform);
		}
	}

	private void SetupEquipItemIcon()
	{
		ClearEquipItemIcon();
		UIHelper.EnableGameObject(EquipRoot, _option.ShowEquipSlot);
		if (!_option.ShowEquipSlot || _cachedHeroInfo == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EquipSlotInfos); i++)
		{
			EquipSlotInfo equipSlotInfo = EquipSlotInfos[i];
			if (equipSlotInfo == null)
			{
				continue;
			}
			EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(equipSlotInfo.EquipSlotIndex);
			UIHelper.EnableGameObject(equipSlotInfo.Sprite_Lock, data == null);
			EquipItemInfo equipItemInfoByPartIndex = _cachedHeroInfo.GetEquipItemInfoByPartIndex(equipSlotInfo.EquipSlotIndex);
			if (equipItemInfoByPartIndex != null)
			{
				ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfoByPartIndex.ItemIndex);
				if (data2 != null)
				{
					ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(equipSlotInfo.Root);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
					itemIconComponent.Open(data2, equipItemInfoByPartIndex, this, _cachedHeroInfo);
					_equipItemIconComponentList.Add(itemIconComponent);
				}
			}
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent equipIconComponent)
	{
	}

	public void OnClickConfirm()
	{
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
		_onConfirm = null;
		Close();
	}

	public void OnClickPreset1()
	{
		ChangePage(1);
	}

	public void OnClickPreset2()
	{
		ChangePage(2);
	}

	public void OnClickPreset3()
	{
		ChangePage(3);
	}

	public void OnClickPreset4()
	{
		ChangePage(4);
	}

	public void OnClickPreset5()
	{
		ChangePage(5);
	}

	public void OnClickPreset6()
	{
		ChangePage(6);
	}

	public void OnShowPreset1()
	{
		SetupTranscendSkillGroupList(1);
	}

	public void OnShowPreset2()
	{
		SetupTranscendSkillGroupList(2);
	}

	public void OnShowPreset3()
	{
		SetupTranscendSkillGroupList(3);
	}

	public void OnShowPreset4()
	{
		SetupTranscendSkillGroupList(4);
	}

	public void OnShowPreset5()
	{
		SetupTranscendSkillGroupList(5);
	}

	public void OnShowPreset6()
	{
		SetupTranscendSkillGroupList(6);
	}

	public void OnHidePreset()
	{
		UIHelper.EnableGameObject(SkillPageRoot, enable: false);
		ClearGroupList();
	}

	public void ShowRuneTooltip()
	{
		if (_runeTooltipInfo != null && !RuneHelper.IsEmptyRunePage(_runeTooltipInfo.HeroInfo, _runeTooltipInfo.RunePage))
		{
			RunehengeTooltip.Open(WidgetRuneTooltip, _runeTooltipInfo);
		}
	}

	public void HideRunetooltip()
	{
		RunehengeTooltip.Hide(WidgetRuneTooltip);
	}

	private PresetButton GetPresetButton(int page)
	{
		if (__Extension_Method_ICollection__.COUNT(PresetButtonList) < page)
		{
			return null;
		}
		return PresetButtonList[page - 1];
	}

	private void ChangePage(int page)
	{
		CurrentSelectPage = page;
		UpdatePresetButton();
	}

	private void UpdatePresetButton()
	{
		_cachedHeroInfo.GetOpenedTranscendSkillPageCount();
		for (int i = 1; i <= MaxPage; i++)
		{
			PresetButton presetButton = GetPresetButton(i);
			if (_cachedHeroInfo.GetTranscendSkillPageInfo(i) != null)
			{
				UIHelper.EnableButton(presetButton.Button, i != CurrentSelectPage);
				UIHelper.SetWidgetGrayScale(presetButton.ButtonSprite, grayScale: false);
				UIHelper.EnableGameObject(presetButton.Button, i != CurrentSelectPage);
				UIHelper.EnableGameObject(presetButton.Select, i == CurrentSelectPage);
			}
			else
			{
				UIHelper.EnableGameObject(presetButton.Button, enable: true);
				UIHelper.EnableButton(presetButton.Button, enable: false, grayscaleIfDisable: true);
				UIHelper.EnableGameObject(presetButton.Select, enable: false);
			}
		}
	}

	private void ClearGroupList()
	{
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
	}

	private void SetupTranscendSkillGroupList(int page)
	{
		ClearGroupList();
		UIHelper.EnableGameObject(SkillPageRoot, enable: true);
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("TotalTranscendSkill");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (valueInt2 == 0 || valueInt == 0)
		{
			return;
		}
		int num = Mathf.Min(valueInt2, valueInt);
		for (int i = 1; i <= num; i++)
		{
			if (__Extension_Method_ICollection__.COUNT(_cachedHeroInfo.CreatureData.GetTranscendSkill(i)) != 0)
			{
				HeroTranscendSkillGroupComponent heroTranscendSkillGroupComponent = HeroTranscendSkillGroupComponent.PrefabAuto.Factory.Create(SkillGroupTable.transform);
				heroTranscendSkillGroupComponent.Open(_cachedHeroInfo, page, i, null, readOnly: true);
				_ListTranscendSkillGroup.Add(heroTranscendSkillGroupComponent);
			}
		}
		SkillGroupTable.Reposition();
	}

	CachedHeroInfo HeroRunehengeIcon.IDataProvider.GetHeroInfo()
	{
		return _cachedHeroInfo;
	}
}
