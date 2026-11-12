using System;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceAwakenWindow : ValanceEnhanceBase, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public interface IListener
	{
		void OnClickAwaken(int slotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceAwakenWindow";

	public Transform ResultEquipItemRoot;

	public Transform MaterialEquipItemRoot;

	public UILabel LabelReqMaterialCount;

	public UILabel LabelReqGold;

	public UILabel LabelOriginalPrice;

	public UILabelAligner PriceLabelAligner;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	public UISprite EventButtonImage;

	public UIButton Button_Awaken;

	public UIWidget IconProxyRoot;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	private IListener _listener;

	private ItemIconComponent _resultComp;

	private ItemIconComponent _materialComp;

	private StatListComponent _statListComponent;

	private IconProxy _iconProxy;

	public ValanceAwakenData ValanceAwakenData { get; private set; }

	public int BeforeStar { get; private set; }

	public int AfterStar { get; private set; }

	public StatData BeforeStat { get; private set; }

	public StatData AfterStat { get; private set; }

	public override bool InvenDefaultFilter(EquipItemInfo info)
	{
		if (info.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (info.GetEquippedHeroIndex() != -1)
		{
			return false;
		}
		if (!Item_Equip_MaxStar.CanTechnoAwaken(info))
		{
			return false;
		}
		return EquipItemFilter.Filter(info.EquipItemData);
	}

	public override void ItemDisabledAction(ItemIconComponent obj)
	{
		EquipItemInfo equipItemInfo = obj.EquipItemInfo;
		if (equipItemInfo != null)
		{
			bool flag = Item_Equip_MaxStar.CanTechnoAwaken(equipItemInfo);
			obj.SetDisabled(!flag);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && !(TargetComponent == null) && ValanceAwakenData != null)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
			if (data != null && ValanceAwakenData.ItemCode1 == data.Code)
			{
				SetupMaterialIcon();
				SetButton();
			}
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo != null && !(TargetComponent == null) && changedInfo.UserStatType == UserManager.UserStatType.Gold)
		{
			SetReqGoldLabel();
			SetButton();
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo == null || TargetComponent == null)
		{
			return;
		}
		if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Add)
		{
			EquipItemInfo equipItemInfo = changedInfo.EquipItemInfo;
			if (equipItemInfo != null && Item_Equip_MaxStar.CanTechnoAwaken(equipItemInfo))
			{
				SetInvenItemList();
			}
		}
		else if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Star)
		{
			Refresh();
		}
	}

	public void Open(IListener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			Init();
		}, null);
	}

	public void OpenCutomCinema(EquipItemInfo itemInfo, IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceAwakenWindowInven", delegate
		{
			Init(itemInfo);
			ItemListPanel.ExternalClickItemIcon(itemInfo, isRefreshTab: true);
		}, null);
	}

	protected override void NodeTreeOpenOnStart()
	{
		base.NodeTreeOpenOnEnd();
		SceneAutoBehaviour<ValanceManagement>.instance.ManageOnOtherWindow();
	}

	protected override void NodeTreeCloseOnEnd()
	{
		base.NodeTreeCloseOnEnd();
		SceneAutoBehaviour<ValanceManagement>.instance.ManageReOpen();
	}

	public new void Init(EquipItemInfo equipItemInfo = null)
	{
		base.Init(equipItemInfo);
		SetIconProxy();
		SetReqGoldLabel();
		SetupMaterialIcon();
		SetButton();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	public override void SelectedTargetItem(EquipItemInfo equipItemInfo)
	{
		base.SelectedTargetItem(equipItemInfo);
		SetupResultIcon();
		SetupMaterialIcon();
		SetReqGoldLabel();
		SetButton();
		SetItemStat();
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

	private void SetupResultIcon()
	{
		if (base.EquipItemInfo == null)
		{
			SprkTechnomagicAwaken.Setup(this);
			return;
		}
		if (_resultComp == null)
		{
			_resultComp = ItemIconComponent.PrefabAuto.Factory.Create(ResultEquipItemRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_resultComp.transform);
		}
		else
		{
			_resultComp.SetStarBlink(isEnabled: false);
		}
		int num = base.EquipItemInfo.CLI_GetMaxStar_for_Awaken();
		BeforeStar = base.EquipItemInfo.Star;
		AfterStar = ((num <= BeforeStar) ? num : (BeforeStar + 1));
		BeforeStat = StatCalculator.GetEquipItemStatData(base.EquipItemInfo.ItemIndex, BeforeStar, base.EquipItemInfo.Level);
		AfterStat = StatCalculator.GetEquipItemStatData(base.EquipItemInfo.ItemIndex, AfterStar, base.EquipItemInfo.Level);
		EquipItemInfo equipItemInfo = base.EquipItemInfo.Clone();
		equipItemInfo.Star = AfterStar;
		equipItemInfo.SoulWeaponInfo = base.EquipItemInfo.SoulWeaponInfo;
		equipItemInfo.Identified = 1;
		equipItemInfo.ExtraOptionIndex1 = base.EquipItemInfo.ExtraOptionIndex1;
		equipItemInfo.ExtraOptionIndex2 = base.EquipItemInfo.ExtraOptionIndex2;
		equipItemInfo.ExtraOptionStep1 = base.EquipItemInfo.ExtraOptionStep1;
		equipItemInfo.ExtraOptionStep2 = base.EquipItemInfo.ExtraOptionStep2;
		int detailIndex = base.EquipItemData.DetailIndex;
		byte tier = (byte)base.EquipItemData.Tier;
		CreatureTagType tagType = base.EquipItemData.TagType.FirstOrDefault();
		ValanceAwakenData = GameRoot.Instance.ValanceAwakenDataContainer.GetData(new ValanceAwakenDataKey(tier, detailIndex, tagType, AfterStar));
		_resultComp.Open(base.ItemData, equipItemInfo, null);
		_resultComp.SetStarBlink();
		SprkTechnomagicAwaken.Setup(this);
	}

	private void SetupMaterialIcon()
	{
		string code = ((ValanceAwakenData != null) ? ValanceAwakenData.ItemCode1 : "");
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		if (dataByCode == null)
		{
			UIHelper.SetLabelText(LabelReqMaterialCount, "");
			return;
		}
		if (_materialComp == null)
		{
			_materialComp = ItemIconComponent.PrefabAuto.Factory.Create(MaterialEquipItemRoot);
			_materialComp.transform.localScale = Vector3.one;
			_materialComp.transform.localPosition = Vector3.zero;
		}
		_materialComp.Open(dataByCode);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(ValanceAwakenData.ItemCode1);
		int itemCount2 = ValanceAwakenData.ItemCount1;
		Color color = ((itemCount2 <= itemCount) ? ColorGoldEnough : ColorGoldNotEnough);
		string text = $"{itemCount} / {itemCount2}";
		UIHelper.SetLabelColor(LabelReqMaterialCount, color);
		UIHelper.SetLabelText(LabelReqMaterialCount, text);
	}

	public void SetReqGoldLabel()
	{
		if (TargetComponent == null)
		{
			UIHelper.SetLabelText(LabelReqGold, 0);
			UIHelper.EnableGameObject(LabelOriginalPrice, enable: false);
			UIHelper.EnableGameObject(EventButtonImage, enable: false);
			UIHelper.SetLabelColor(LabelReqGold, ColorGoldEnough);
			if (PriceLabelAligner != null)
			{
				PriceLabelAligner.Align();
			}
			return;
		}
		int num = ((ValanceAwakenData != null) ? ValanceAwakenData.ReqGold : 0);
		long num2 = num;
		int num3 = 0;
		bool flag = num3 != 0;
		UIHelper.EnableGameObject(EventButtonImage, flag);
		long num4;
		if (!flag)
		{
			num4 = num2;
		}
		else
		{
			int num5 = num3;
			long val = checked(num2 - num5);
			int num6 = 0;
			num4 = Math.Max(val, num6);
		}
		long num7 = num4;
		Color color = ((num7 <= GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold) ? ColorGoldEnough : ColorGoldNotEnough);
		UIHelper.SetLabelColor(LabelReqGold, color);
		UIHelper.SetLabelText(LabelReqGold, Vespa_Int_Extension_Method.ToCommaString(num7));
	}

	public void SetItemStat()
	{
		if (StatChangeScrollView == null || StatChangePanel == null)
		{
			return;
		}
		if (_statListComponent != null)
		{
			ClearOptitonStat();
		}
		_statListComponent = StatListComponent.Create(StatChangePanel, 2, isValance: true);
		bool flag = _statListComponent.AddStatChange(BeforeStat, AfterStat, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade);
		if (base.EquipItemData != null)
		{
			int num = 1;
			while (true)
			{
				int fixedOptionIndex = base.EquipItemData.GetFixedOptionIndex(num);
				if (fixedOptionIndex == 0)
				{
					break;
				}
				bool flag2 = _statListComponent.AddOptionChangeSkill(fixedOptionIndex, BeforeStar, AfterStar, StatListComponent.EOptionChangeTemplate.ForStarUpgrade);
				flag = flag || flag2;
				num++;
			}
		}
		foreach (EquipItemOptionInfo optionInfo in base.EquipItemInfo.GetOptionInfoList())
		{
			_statListComponent.AddOptionChange(optionInfo, BeforeStar, AfterStar, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade);
		}
		if ((bool)StatChangeScrollView)
		{
			StatChangeScrollView.ResetPosition();
		}
	}

	public void SetButton()
	{
		bool flag = TargetComponent != null;
		bool flag2 = IsEnoughGold();
		bool flag3 = IsEnoughMaterial();
		UIHelper.EnableButton(Button_Awaken, flag && flag2 && flag3, grayscaleIfDisable: true);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!(TargetComponent == null))
		{
			if (!Item_Equip_MaxStar.CanTechnoAwaken(TargetComponent.EquipItemInfo))
			{
				ClearSeletedTarget();
				ItemListPanel.ResetItemList();
			}
			else
			{
				ItemListPanel.ExternalClickItemIcon(TargetComponent.EquipItemInfo);
			}
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseIconProxy();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public override void ClearSeletedTarget()
	{
		base.ClearSeletedTarget();
		SetReqGoldLabel();
		SetButton();
		ClearSeletedResult();
	}

	public void ClearSeletedResult()
	{
		SprkTechnomagicAwaken.Clear(this);
		if (ValanceAwakenData != null)
		{
			ValanceAwakenData = null;
		}
		ClearResultIcon();
		ClearMaterialIcon();
		ClearOptitonStat();
	}

	private void ClearResultIcon()
	{
		if (_resultComp != null)
		{
			FactoryBase.SafeReleaseComponent(ref _resultComp);
		}
	}

	private void ClearMaterialIcon()
	{
		if (_materialComp != null)
		{
			UIHelper.SetLabelText(LabelReqMaterialCount, "");
			FactoryBase.SafeReleaseComponent(ref _materialComp);
		}
	}

	private void ClearOptitonStat()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	private void ReleaseIconProxy()
	{
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
	}

	private bool IsEnoughMaterial()
	{
		if (ValanceAwakenData == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(ValanceAwakenData.ItemCode1) >= ValanceAwakenData.ItemCount1;
	}

	private bool IsEnoughGold()
	{
		if (ValanceAwakenData == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= ValanceAwakenData.ReqGold;
	}

	public void OnClickAwaken()
	{
		SprkTechnomagicAwaken.Remember(this);
		if (_listener != null)
		{
			int slotIndex = TargetComponent.EquipItemInfo.SlotIndex;
			_listener.OnClickAwaken(slotIndex);
		}
	}
}
