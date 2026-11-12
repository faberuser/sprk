using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NValance;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceCraftWindow : BaseCinemaWindow, ValanceCraftFilterPanel.IListener, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>
{
	public interface IListener
	{
		void OnClickCraft(ValanceCraftDataKey key, int extraEquipOptionIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceCraftWindow";

	public GameObject GoSelect;

	public UILabel LabelMaterialCount;

	public UILabel LabelDiscount;

	public UILabel LabelReqGold;

	public GameObject SubMaterialGroupRoot;

	public Transform TransSubMaterial;

	public UILabel LabelSubMaterialCount;

	public UILabelAligner LabelAligner;

	public UIWidget IconProxyRoot;

	public UIButton ButtonCraft;

	public GameObject ButtonEventGo;

	public UIReuseScrollGrid CraftResuseGrid;

	public UIScrollGrid CraftScrollGrid;

	public ValanceCraftFilterPanel FilterPanel;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	public Transform MaterialRoot;

	private IconProxy _iconProxy;

	private List<ItemIconComponent> _listCraftComponent = new List<ItemIconComponent>();

	private ItemIconComponent _mainMaterialItemComponent;

	private ItemIconComponent _subMaterialComponent;

	private IListener _listener;

	private ValanceCraftData _selectedCraftData;

	private int[] _equipOptionIndices;

	public void Open(IListener listener)
	{
		_listener = listener;
		Open(Init, delegate
		{
			FilterPanel.Reposition();
		});
	}

	public void OpenCutomCinema(IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceCraftWindowInven", Init, delegate
		{
			FilterPanel.Reposition();
		});
	}

	public void Init()
	{
		SetFilterGroup();
		SetCraftData(ValanceCraftDataKey.DefaultKey, null);
		SetIconProxy();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	public void SetFilterGroup()
	{
		FilterPanel.SetListener(this);
		FilterPanel.SetFilterGroupComponent();
		FilterPanel.SetNotice();
	}

	public void SetCraftData(ValanceCraftData craftData)
	{
		bool enable = FilterPanel.SelectedKey.Equals(ValanceCraftDataKey.DefaultKey);
		UIHelper.EnableGameObject(GoSelect, enable);
		if (craftData == null || _selectedCraftData != craftData)
		{
			_selectedCraftData = craftData;
			SetCraftComponent(_selectedCraftData);
			bool flag = SetMetrialComponent(_selectedCraftData);
			bool flag2 = SetReqLabel(_selectedCraftData);
			SetCraftButtonState(flag && flag2);
		}
	}

	public void SetCraftData(ValanceCraftDataKey selectedKey, int[] equipOptionIndices)
	{
		bool enable = selectedKey.Equals(ValanceCraftDataKey.DefaultKey);
		UIHelper.EnableGameObject(GoSelect, enable);
		ValanceCraftData data = GameRoot.Instance.ValanceCraftDataContainer.GetData(selectedKey);
		if (data == null || _selectedCraftData != data || _equipOptionIndices != equipOptionIndices)
		{
			if (_selectedCraftData != data)
			{
				_selectedCraftData = data;
				SetCraftComponent(_selectedCraftData);
			}
			_equipOptionIndices = equipOptionIndices;
			bool flag = SetMetrialComponent(_selectedCraftData, equipOptionIndices);
			bool flag2 = SetReqLabel(_selectedCraftData);
			SetCraftButtonState(flag && flag2);
		}
	}

	public void SetCraftComponent(ValanceCraftData craftData)
	{
		if (craftData == null)
		{
			ReleaseAllCraftComponents();
			return;
		}
		List<string> craftItemCode = GetCraftItemCode(craftData.ResultItemGroupCode);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(craftItemCode))
		{
			ReleaseAllCraftComponents();
			return;
		}
		craftItemCode.Sort(ValanceHelper.CraftComparer.Default);
		int num = __Extension_Method_ICollection__.COUNT(craftItemCode);
		int num2 = __Extension_Method_ICollection__.COUNT(_listCraftComponent);
		for (int num3 = ((num > num2) ? num : num2) - 1; num3 >= 0; num3--)
		{
			string text = ((num > num3) ? craftItemCode[num3] : "");
			GameRoot.Instance.ItemDataContainer.GetDataByCode(text);
			ItemIconComponent itemIconComponent = ((num2 > num3) ? _listCraftComponent[num3] : null);
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				if (itemIconComponent != null)
				{
					FactoryBase.ReleaseComponent(itemIconComponent);
					_listCraftComponent.Remove(itemIconComponent);
				}
			}
			else if (itemIconComponent == null)
			{
				itemIconComponent = UIHelper.AddGridItem(CraftScrollGrid, ItemIconComponent.PrefabAuto);
				_listCraftComponent.Add(itemIconComponent);
			}
		}
		for (int i = 0; i < num; i++)
		{
			string code = craftItemCode[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			_listCraftComponent[i].Open(dataByCode);
		}
		UIHelper.RepositionGrid(CraftScrollGrid);
	}

	private bool SetMaterial(int itemIndex, int reqCount, int addReqCount, EventBonusType discountBonusType, UILabel labelCount, Transform parent, ref ItemIconComponent component)
	{
		if (parent == null)
		{
			return false;
		}
		if (component == null)
		{
			component = ItemIconComponent.PrefabAuto.Factory.Create(parent);
			component.ResetPosition();
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return false;
		}
		component.Open(data);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemIndex);
		int discountedMaterialCount = GetDiscountedMaterialCount(reqCount, discountBonusType);
		int discountedMaterialCount2 = GetDiscountedMaterialCount(addReqCount, EventBonusType.DiscountAdditionalMaterialItemCount);
		int num = discountedMaterialCount + discountedMaterialCount2;
		bool num2 = itemCount >= num;
		Color color = (num2 ? ColorGoldEnough : ColorGoldNotEnough);
		UIHelper.EnableGameObject(labelCount, enable: true);
		UIHelper.SetLabelText(labelCount, $"{itemCount}/{num}");
		UIHelper.SetLabelColor(labelCount, color);
		return num2;
	}

	private int GetDiscountedMaterialCount(int reqMaterialCount, EventBonusType discountBonusType)
	{
		float discountRatio = GetDiscountRatio(discountBonusType);
		reqMaterialCount = (int)Math.Ceiling(Math.Round((float)reqMaterialCount * discountRatio, 2));
		return reqMaterialCount;
	}

	private float GetDiscountRatio(EventBonusType discountBonusType)
	{
		float num = 1f;
		if (!GameRoot.Instance.HeroBonusDataContainer.BonusOwnedHeroIndices.ContainsKey(discountBonusType))
		{
			return num;
		}
		foreach (int item in GameRoot.Instance.HeroBonusDataContainer.BonusOwnedHeroIndices[discountBonusType])
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item);
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
			if (data != null)
			{
				HeroEventBonusData[] heroBonusTypeDatas = data.GetHeroBonusTypeDatas(discountBonusType);
				for (int i = 0; i < heroBonusTypeDatas.Length; i++)
				{
					int num2 = Vespa_String_Extension_Method.TOINT(heroBonusTypeDatas[i].BonusValue[0], 0);
					num *= 1f - (float)num2 * 0.01f;
				}
			}
		}
		return num;
	}

	public bool SetMetrialComponent(ValanceCraftData craftData, int[] equipOptionIndices = null)
	{
		if (craftData == null)
		{
			UIHelper.EnableGameObject(LabelMaterialCount, enable: false);
			UIHelper.EnableGameObject(SubMaterialGroupRoot, enable: false);
			ReleaseMaterialComponents();
			RelaseSubMaterialComponents();
			return false;
		}
		int materialItemIndex = craftData.MaterialItemIndex;
		if (GameRoot.Instance.ItemDataContainer.GetData(materialItemIndex) == null)
		{
			UIHelper.EnableGameObject(LabelMaterialCount, enable: false);
			UIHelper.EnableGameObject(SubMaterialGroupRoot, enable: false);
			ReleaseMaterialComponents();
			RelaseSubMaterialComponents();
			return false;
		}
		bool flag = __Extension_Method_ICollection__.IsNullOrEmpty(equipOptionIndices);
		if (flag)
		{
			UIHelper.EnableGameObject(SubMaterialGroupRoot, enable: false);
			RelaseSubMaterialComponents();
		}
		UIHelper.EnableGameObject(SubMaterialGroupRoot, !flag);
		int addReqCount = ((!flag) ? craftData.AdditionalMaterialItemCount : 0);
		bool num = SetMaterial(materialItemIndex, craftData.MaterialItemCount, addReqCount, EventBonusType.DiscountMaterialItemCount, LabelMaterialCount, MaterialRoot, ref _mainMaterialItemComponent);
		bool flag2 = flag || SetMaterial(craftData.SubMaterialItemIndex, craftData.SubMaterialItemCount, 0, EventBonusType.DiscountSubMaterialItemCount, LabelSubMaterialCount, TransSubMaterial, ref _subMaterialComponent);
		return num && flag2;
	}

	public bool SetReqLabel(ValanceCraftData craftData)
	{
		int num = 0;
		long num2 = num;
		if (craftData != null)
		{
			int reqGold = craftData.ReqGold;
			num2 = reqGold;
		}
		bool num3 = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= num2;
		Color color = (num3 ? ColorGoldEnough : ColorGoldNotEnough);
		int eventDiscount = GetEventDiscount();
		long num4 = eventDiscount;
		int num5 = 0;
		bool flag = (ulong)num4 > (ulong)num5;
		UIHelper.EnableGameObject(LabelDiscount, flag);
		UIHelper.EnableGameObject(ButtonEventGo, flag);
		if (flag)
		{
			UIHelper.SetLabelText(LabelDiscount, Vespa_Int_Extension_Method.ToCommaString(num4));
		}
		UIHelper.SetLabelText(LabelReqGold, Vespa_Int_Extension_Method.ToCommaString(num2));
		UIHelper.SetLabelColor(LabelReqGold, color);
		if (LabelAligner != null)
		{
			LabelAligner.Align();
		}
		return num3;
	}

	public void SetCraftButtonState(bool isEnable)
	{
		UIHelper.EnableButton(ButtonCraft, isEnable, grayscaleIfDisable: true);
	}

	public void SetIconProxy()
	{
		if (!(IconProxyRoot == null))
		{
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconProxyRoot);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(CurrencyType.Gold);
			}
		}
	}

	private int GetEventDiscount()
	{
		return 0;
	}

	public List<string> GetCraftItemCode(string resultItemCode)
	{
		return GameRoot.Instance.ItemGroupDataContainer.GetData(resultItemCode)?.GetItemCodeList();
	}

	public void Refresh()
	{
		FilterPanel.SetNotice();
		bool flag = SetReqLabel(_selectedCraftData);
		bool flag2 = SetMetrialComponent(_selectedCraftData, _equipOptionIndices);
		SetCraftButtonState(flag && flag2);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FilterPanel.Close();
		_selectedCraftData = null;
		_equipOptionIndices = null;
		ReleaseAllCraftComponents();
		ReleaseMaterialComponents();
		RelaseSubMaterialComponents();
		ReleaseIconProxy();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	private void ReleaseIconProxy()
	{
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
	}

	public void ReleaseAllCraftComponents()
	{
		FactoryBase.ReleaseAndClearList(_listCraftComponent);
	}

	public void ReleaseMaterialComponents()
	{
		FactoryBase.SafeReleaseComponent(ref _mainMaterialItemComponent);
	}

	public void RelaseSubMaterialComponents()
	{
		FactoryBase.SafeReleaseComponent(ref _subMaterialComponent);
	}

	public void OnClickCraft()
	{
		if (_selectedCraftData == null || _listener == null)
		{
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(ResultType.EquipItemFull);
			MessagePopup.OpenEnabled(text, message, delegate
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
				SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
			}, null, "MOVE_BAG", bYesEnabled: true);
		}
		else if (!__Extension_Method_ICollection__.IsNullOrEmpty(_equipOptionIndices))
		{
			int equipOptionIndex = ValanceHelper.GetEquipOptionIndex(_selectedCraftData, _equipOptionIndices);
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(equipOptionIndex);
			string text2 = "";
			if (data != null)
			{
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(0, 0);
				text2 = StatData.GetOptionDisplay(optionType, optionValue, opened: true, 0, appendColor: false);
				if (!String_Extension_Method.IsNullOrEmpty(text2) && text2[text2.Length - 1] == '.')
				{
					text2 = text2.Substring(0, text2.Length - 1);
				}
			}
			string text3 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("VALANCECRAFT_ADDTIONALSKILL_POPUP", text2);
			MessagePopup3Button.Open(text3, stringDirect, null, delegate
			{
				_listener.OnClickCraft(FilterPanel.SelectedKey, equipOptionIndex);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			_listener.OnClickCraft(FilterPanel.SelectedKey, 0);
		}
	}

	void ValanceCraftFilterPanel.IListener.OnClickEquipFilter(ValanceCraftDataKey selectedKey, int[] equipOptionIndices)
	{
		SetCraftData(selectedKey, equipOptionIndices);
	}

	public void SetCenterMessage(string key)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect(key));
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (_selectedCraftData != null && (_selectedCraftData.MaterialItemIndex == changedInfo.ItemIndex || _selectedCraftData.SubMaterialItemIndex == changedInfo.ItemIndex))
		{
			Refresh();
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (_selectedCraftData != null && changedInfo.UserStatType == UserManager.UserStatType.Gold)
		{
			Refresh();
		}
	}
}
