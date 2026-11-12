using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipOptionUpgrade : EquipEnhanceBase, IUserObserver<ItemInfo>, IUserObserver
{
	public interface Listener
	{
		void OnUpgrade(EquipOptionUpgrade window);

		void OnMaxUpgrade(EquipOptionUpgrade window);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Equip/EquipOptionUpgrade";

	public UISimpleGrid MaterialItemGrid;

	public UILabel PriceLabel;

	public UILabel OriginalPriceLabel;

	public UILabelAligner PriceLabelAligner;

	public UIButton UpgradeButton;

	public UIButton MaxUpgradeButton;

	public Transform StatGaugeRoot;

	public GameObject MaterialCountRoot;

	public UISprite DefaultMaterialCountBG;

	public UISprite SaleMaterialCountBG;

	public UILabel DefaultMaterialCountLabel;

	public UILabel OwnMaterialCountLabel;

	public UILabel ReqMaterialCountLabel;

	public UILabel OriginalReqMaterialCountLabel;

	public UILabel DiscountReqMaterialCountLabel;

	public UILabelAligner MaterialCountAligner;

	public Color MaterialEnoughColor = new Color(20f / 51f, 1f, 20f / 51f);

	public Color MaterialNotEnoughColor = new Color(1f, 20f / 51f, 20f / 51f);

	public List<UILabel> MaterialCountColorLabels = new List<UILabel>();

	public UISprite EventMaxButtonImage;

	public LocalizeLinkData UpgradeText;

	private Listener _listener;

	private ItemIconComponent _materialItemIconComponent;

	private StatGaugeComponent _statGaugeComponent;

	private long itemReqCount { get; set; }

	protected override EventBonusType BonusType => EventBonusType.DiscountEquipItemOptionUpgrade;

	public void Init(Listener listener)
	{
		InitBase();
		_listener = listener;
		InitMaterialItemIcon();
	}

	public override void Reset()
	{
		base.Reset();
		ClearMaterialInfo();
		ClearStatGauge();
	}

	private void ClearMaterialInfo()
	{
		FactoryBase.SafeReleaseComponent(ref _materialItemIconComponent);
		UIHelper.SetLabelText(PriceLabel, 0);
		UIHelper.EnableGameObject(OriginalPriceLabel, enable: false);
		UIHelper.EnableButton(UpgradeButton, enable: false);
		UIHelper.EnableButton(MaxUpgradeButton, enable: false);
		UIHelper.EnableGameObject(MaterialCountRoot, enable: false);
	}

	private void ClearStatGauge()
	{
		FactoryBase.SafeReleaseComponent(ref _statGaugeComponent);
	}

	public string GetUpgradeText()
	{
		if (UpgradeText == null)
		{
			return null;
		}
		return UpgradeText.GetLocalize();
	}

	public void OnClickUpgrade()
	{
		if (_listener != null)
		{
			_listener.OnUpgrade(this);
		}
	}

	public void OnClickMaxUpgrade()
	{
		if (base.SelectedEquipItemInfo == null)
		{
			return;
		}
		EquipOptionMaxUpgradePopup.Open(base.SelectedEquipItemInfo, delegate
		{
			if (_listener != null)
			{
				_listener.OnMaxUpgrade(this);
			}
		});
	}

	public static bool CanUpgrade(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.IsEndOptionStep())
		{
			return false;
		}
		if (equipItemInfo.IsMaxOptionStep())
		{
			return false;
		}
		return true;
	}

	public override bool CanEnhance(EquipItemInfo equipItemInfo)
	{
		return CanUpgrade(equipItemInfo);
	}

	public override void OnTargetItemSelected(EquipItemInfo equipItemInfo)
	{
		base.OnTargetItemSelected(equipItemInfo);
		UpdateMaterialUI(equipItemInfo);
		CreateStatGauge(equipItemInfo);
	}

	protected override ItemIconComponent CreateInventoryItemComponent(EquipItemInfo equipItemInfo, bool noTechnomagic = false)
	{
		if (CanEnhance(equipItemInfo))
		{
			return base.CreateInventoryItemComponent(equipItemInfo, noTechnomagic);
		}
		return null;
	}

	private void CreateStatGauge(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			ClearStatGauge();
			return;
		}
		if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex) == null)
		{
			ClearStatGauge();
			VespaDebug.LogError("EquipItemData not found : index=" + equipItemInfo.ItemIndex);
			return;
		}
		ClearStatGauge();
		_statGaugeComponent = StatGaugeComponent.PrefabAuto.Factory.Create(StatGaugeRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_statGaugeComponent.transform);
		foreach (EquipItemOptionInfo optionInfo in equipItemInfo.GetOptionInfoList())
		{
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data != null)
			{
				int optionValueStep = data.OptionValueStep;
				int num = Mathf.Min(optionInfo.OptionStep, optionValueStep);
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(equipItemInfo.Star, num);
				int optionValueMax = data.GetOptionValueMax(equipItemInfo.Star);
				_statGaugeComponent.AddStat(optionType, optionValue, optionValueMax, num, optionValueStep);
			}
		}
	}

	public ITask CreateChangedOptionStatAnimation()
	{
		if (base.SelectedEquipItemInfo == null)
		{
			return null;
		}
		ParallelTask parallelTask = new ParallelTask();
		foreach (EquipItemOptionInfo optionInfo in base.SelectedEquipItemInfo.GetOptionInfoList())
		{
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data == null)
			{
				continue;
			}
			int index = optionInfo.SlotIndex - 1;
			StatGaugeInfo statGaugeInfo = _statGaugeComponent.GetStatGaugeInfo(index);
			if (statGaugeInfo == null)
			{
				VespaDebug.LogError("StatGauge not found : index=" + index);
				continue;
			}
			if ((OperationType)(object)statGaugeInfo.StatType != data.OptionType)
			{
				VespaDebug.LogError("StatType mismatch : info.StatType=" + statGaugeInfo.StatType?.ToString() + " optionData.OptionType=" + data.OptionType);
				continue;
			}
			int optionStep = optionInfo.OptionStep;
			int optionValue = data.GetOptionValue(base.SelectedEquipItemInfo.Star, optionStep);
			ITask task = statGaugeInfo.CreateGaugeAnimation(optionValue, optionStep);
			if (task != null)
			{
				parallelTask.AddTask(task);
			}
		}
		return parallelTask;
	}

	private int GetMaterialCount()
	{
		if (_materialItemIconComponent == null || _materialItemIconComponent.ItemData == null)
		{
			return 0;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(_materialItemIconComponent.ItemData.Index);
	}

	private void InitMaterialItemIcon()
	{
		EquipOptionUpgradeData data = GameRoot.Instance.EquipOptionUpgradeDataContainer.GetData(1, 1001);
		if (data == null)
		{
			ClearMaterialInfo();
			VespaDebug.LogError("OptionUpgradeData not found");
			return;
		}
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(data.MaterialItemIndex);
		if (data2 == null)
		{
			ClearMaterialInfo();
			VespaDebug.LogError("Material ItemData not found");
			return;
		}
		ClearMaterialInfo();
		_materialItemIconComponent = UIHelper.AddGridItem(MaterialItemGrid, ItemIconComponent.PrefabAuto);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_materialItemIconComponent.transform);
		_materialItemIconComponent.Open(data2);
		int materialCount = GetMaterialCount();
		int originalCount = -1;
		int num = -1;
		SetMaterialCount(isDefalut: true, isSale: false, materialCount, num, originalCount);
	}

	private void UpdateMaterialUI(EquipItemInfo equipItemInfo)
	{
		int materialCount = GetMaterialCount();
		if (equipItemInfo == null)
		{
			int originalCount = -1;
			int num = -1;
			SetMaterialCount(isDefalut: true, isSale: false, materialCount, num, originalCount);
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		if (data == null)
		{
			VespaDebug.LogError("EquipItemData not found : index=" + equipItemInfo.ItemIndex);
			return;
		}
		EquipOptionUpgradeData data2 = GameRoot.Instance.EquipOptionUpgradeDataContainer.GetData(data.SubData.Tier, data.SubData.DetailIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("OptionUpgradeData not found : index=" + equipItemInfo.ItemIndex + " tier=" + data.SubData.Tier + " detailIndex=" + data.SubData.DetailIndex);
			return;
		}
		int reqGold = data2.ReqGold;
		long num2 = reqGold;
		int materialItemCount = data2.MaterialItemCount;
		long num3 = materialItemCount;
		bool flag = false;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountEquipItemOptionUpgrade, null);
		bool flag2;
		bool flag3;
		checked
		{
			if (effectiveEvent != null)
			{
				long num4 = num2;
				int bonusPercentage = effectiveEvent.BonusPercentage;
				long num5 = num4 * bonusPercentage;
				int num6 = 100;
				long num7 = unchecked(num5 / num6);
				long val = num2 - num7;
				int num8 = 0;
				num2 = System.Math.Max(val, num8);
				long num9 = num3;
				int bonusPercentage2 = effectiveEvent.BonusPercentage;
				long num10 = num9 * bonusPercentage2;
				int num11 = 100;
				num7 = unchecked(num10 / num11);
				long val2 = num3 - num7;
				int num12 = 0;
				num3 = System.Math.Max(val2, num12);
				flag = true;
			}
			flag2 = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= num2;
			long num13 = num3;
			int num14 = materialCount;
			flag3 = num14 >= num13;
			UIHelper.EnableGameObject(OriginalPriceLabel, flag);
			if (flag)
			{
				UILabel originalPriceLabel = OriginalPriceLabel;
				int reqGold2 = data2.ReqGold;
				UIHelper.SetLabelText(originalPriceLabel, Vespa_Int_Extension_Method.ToCommaString(reqGold2));
			}
			UIHelper.SetLabelText(PriceLabel, flag2 ? Vespa_Int_Extension_Method.ToCommaString(num2) : NGUIText.EncodeColor(Vespa_Int_Extension_Method.ToCommaString(num2), Color.red));
		}
		UIHelper.EnableButton(UpgradeButton, flag3 && flag2);
		UIHelper.EnableButton(MaxUpgradeButton, flag3 && flag2);
		if (PriceLabelAligner != null)
		{
			PriceLabelAligner.Align();
		}
		itemReqCount = num3;
		SetMaterialCount(isDefalut: false, flag, materialCount, itemReqCount, data2.MaterialItemCount);
	}

	public int GetMaterialItemIndex()
	{
		if (_materialItemIconComponent == null)
		{
			return 0;
		}
		return _materialItemIconComponent.ItemData.Index;
	}

	public long GetMaterialItemReqCount()
	{
		if (_materialItemIconComponent == null)
		{
			int num = 0;
			return num;
		}
		return itemReqCount;
	}

	private void SelectMaterialCountBG(bool isSale)
	{
		UIHelper.EnableGameObject(DefaultMaterialCountBG, !isSale);
		UIHelper.EnableGameObject(SaleMaterialCountBG, isSale);
	}

	private void SetMaterialCount(bool isDefalut, bool isSale, int ownCount, long reqCount = -1, int originalCount = -1)
	{
		UIHelper.EnableGameObject(MaterialCountRoot, enable: true);
		if (isDefalut)
		{
			UIHelper.EnableGameObject(DefaultMaterialCountLabel, enable: true);
			UIHelper.EnableGameObject(MaterialCountAligner, enable: false);
			UIHelper.EnableGameObject(DiscountReqMaterialCountLabel, enable: false);
			UIHelper.SetLabelText(DefaultMaterialCountLabel, ownCount);
			SelectMaterialCountBG(isSale: false);
			return;
		}
		Color materialLabelColor = ((ownCount >= reqCount) ? MaterialEnoughColor : MaterialNotEnoughColor);
		if (isSale)
		{
			UIHelper.EnableGameObject(DefaultMaterialCountLabel, enable: false);
			UIHelper.EnableGameObject(MaterialCountAligner, enable: true);
			UIHelper.EnableGameObject(DiscountReqMaterialCountLabel, enable: true);
			UIHelper.EnableGameObject(ReqMaterialCountLabel, enable: false);
			UIHelper.EnableGameObject(OriginalReqMaterialCountLabel, enable: true);
			UIHelper.SetLabelText(OwnMaterialCountLabel, ownCount);
			UIHelper.SetLabelText(OriginalReqMaterialCountLabel, originalCount);
			UIHelper.SetLabelText(DiscountReqMaterialCountLabel, reqCount);
			SetMaterialLabelColor(materialLabelColor);
			SelectMaterialCountBG(isSale: true);
		}
		else
		{
			UIHelper.EnableGameObject(DefaultMaterialCountLabel, enable: false);
			UIHelper.EnableGameObject(MaterialCountAligner, enable: true);
			UIHelper.EnableGameObject(DiscountReqMaterialCountLabel, enable: false);
			UIHelper.EnableGameObject(ReqMaterialCountLabel, enable: true);
			UIHelper.EnableGameObject(OriginalReqMaterialCountLabel, enable: false);
			UIHelper.SetLabelText(OwnMaterialCountLabel, ownCount);
			UIHelper.SetLabelText(ReqMaterialCountLabel, reqCount);
			SetMaterialLabelColor(materialLabelColor);
			SelectMaterialCountBG(isSale: false);
		}
		if ((bool)MaterialCountAligner)
		{
			MaterialCountAligner.Align();
		}
	}

	private void SetMaterialLabelColor(Color color)
	{
		MaterialCountColorLabels.ForEach(delegate(UILabel label)
		{
			UIHelper.SetLabelColor(label, color);
		});
	}

	public override void OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		base.OnChanged(changedInfo);
		if (changedInfo != null && !CanEnhance(changedInfo.EquipItemInfo))
		{
			RemoveInventoryItemComponent(changedInfo.EquipItemInfo);
			InventoryEquipItemGrid.Grid.Reposition();
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && (bool)_materialItemIconComponent && _materialItemIconComponent.ItemData.Index == changedInfo.ItemIndex)
		{
			UpdateMaterialUI(base.SelectedEquipItemInfo);
		}
	}

	public override void ShowEventRoot(bool isShow)
	{
		base.ShowEventRoot(isShow);
		UIHelper.EnableGameObject(EventMaxButtonImage, isShow);
	}
}
