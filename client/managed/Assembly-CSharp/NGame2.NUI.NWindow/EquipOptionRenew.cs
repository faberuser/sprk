using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipOptionRenew : EquipEnhanceBase, EquipAwakenScene.IDataProvider
{
	public interface Listener
	{
		void RenewEquipOption(EquipOptionRenew window, int equipItemSlotIndex, int optionSlotIndex);

		void OpenLastRenewResult(EquipOptionRenew window, int equipItemSlotIndex, int optionSlotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Equip/EquipOptionRenew";

	public Transform StatGaugeRoot;

	public LocalizeLinkData UpgradeText;

	public UIBaseGrid RenewableOptionGrid;

	public Color RenewableOptionColor;

	private Listener _listener;

	private StatGaugeComponent _statGaugeComponent;

	private List<SingleStatComponent> _renewableOptionList = new List<SingleStatComponent>();

	protected override EventBonusType BonusType => EventBonusType.DiscountEquipItemOptionRenew;

	public void Init(Listener listener)
	{
		InitBase();
		_listener = listener;
		UpdateOptionButtonState();
		CheckLastRenewResult();
	}

	public override void Reset()
	{
		base.Reset();
		ClearStatGauge();
		ClearRenewableOption();
		SprkReforgeUntil.ClearTarget();
	}

	private void ClearStatGauge()
	{
		SprkReforgeUntil.ReleaseGauge();
		FactoryBase.SafeReleaseComponent(ref _statGaugeComponent);
	}

	private void ClearRenewableOption()
	{
		SprkReforgeUntil.ReleasePool();
		FactoryBase.ReleaseAndClearList(_renewableOptionList);
	}

	private void Refresh()
	{
		CreateStatGauge();
		CreateRenewableOption();
		UpdateOptionButtonState();
	}

	public string GetUpgradeText()
	{
		if (UpgradeText == null)
		{
			return null;
		}
		return UpgradeText.GetLocalize();
	}

	private void UpdateOptionButtonState()
	{
		if (!_statGaugeComponent)
		{
			return;
		}
		int num = 0;
		while (true)
		{
			StatGaugeInfo statGaugeInfo = _statGaugeComponent.GetStatGaugeInfo(num);
			if (statGaugeInfo == null)
			{
				break;
			}
			int slotIndex = num + 1;
			if (base.SelectedEquipItemInfo == null)
			{
				statGaugeInfo.SelectRoot(StatGaugeInfo.StateType.Disabled);
			}
			else
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.SelectedEquipItemInfo.ItemIndex);
				if (data == null)
				{
					break;
				}
				EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
				if (data2 == null)
				{
					break;
				}
				bool flag = base.SelectedEquipItemInfo.IsRenewedOption(slotIndex);
				StatGaugeInfo.StateType stateType = StatGaugeInfo.StateType.None;
				stateType = ((data2.RenewableOptionCount > base.SelectedEquipItemInfo.GetRenewedOptionCount()) ? (flag ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.None) : (flag ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.Disabled));
				statGaugeInfo.SelectRoot(stateType);
			}
			num++;
		}
	}

	private void CheckLastRenewResult()
	{
		int renewOptionEquipItemSlotIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionEquipItemSlotIndex;
		if (renewOptionEquipItemSlotIndex == 0)
		{
			return;
		}
		int renewOptionResultIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultIndex;
		if (renewOptionResultIndex == 0)
		{
			return;
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(renewOptionEquipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("Failed to find equip item of renew : equipItemSlotIndex=" + renewOptionEquipItemSlotIndex + " renewOptionResultIndex=" + renewOptionResultIndex);
			return;
		}
		if (equipItemInfo.GetRenewedOptionCount() == 0)
		{
			VespaDebug.LogError("RenewOptionSlotIndex = 0 : equipItemSlotIndex=" + renewOptionEquipItemSlotIndex + " renewOptionResultIndex=" + renewOptionResultIndex);
			return;
		}
		int renewOptionSlotIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionSlotIndex;
		if (renewOptionSlotIndex != 0)
		{
			ItemIconComponent equipItemIconComponent = GetEquipItemIconComponent(renewOptionEquipItemSlotIndex);
			if (equipItemIconComponent != null)
			{
				OnClickEquipItem(equipItemIconComponent);
			}
			if (_listener != null)
			{
				_listener.OpenLastRenewResult(this, renewOptionEquipItemSlotIndex, renewOptionSlotIndex);
			}
		}
	}

	public static bool CanRenew(EquipItemInfo equipItemInfo)
	{
		EquipItemData equipItemData = equipItemInfo.EquipItemData;
		if (equipItemData == null)
		{
			return false;
		}
		EquipItemDetailData data = GameRoot.Instance.EquipItemDetailDataContainer.GetData(equipItemData.DetailIndex);
		if (data == null)
		{
			return false;
		}
		if (data.RenewableOptionCount > 0)
		{
			return equipItemInfo.HasOption();
		}
		return false;
	}

	public override bool CanEnhance(EquipItemInfo equipItemInfo)
	{
		return CanRenew(equipItemInfo);
	}

	public override void OnTargetItemSelected(EquipItemInfo equipItemInfo)
	{
		base.OnTargetItemSelected(equipItemInfo);
		Refresh();
	}

	protected override ItemIconComponent CreateInventoryItemComponent(EquipItemInfo equipItemInfo, bool noTechnomagic = false)
	{
		if (CanEnhance(equipItemInfo))
		{
			return base.CreateInventoryItemComponent(equipItemInfo, noTechnomagic);
		}
		return null;
	}

	private void CreateStatGauge()
	{
		if (base.SelectedEquipItemInfo == null)
		{
			ClearStatGauge();
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.SelectedEquipItemInfo.ItemIndex);
		if (data == null)
		{
			ClearStatGauge();
			VespaDebug.LogError("EquipItemData not found : index=" + base.SelectedEquipItemInfo.ItemIndex);
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null)
		{
			ClearStatGauge();
			VespaDebug.LogError("EquipItemDetailData not found : index=" + data.SubData.DetailIndex);
			return;
		}
		ClearStatGauge();
		_statGaugeComponent = StatGaugeComponent.ButtonPrefabAuto.Factory.Create(StatGaugeRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_statGaugeComponent.transform);
		foreach (EquipItemOptionInfo optionInfo in base.SelectedEquipItemInfo.GetOptionInfoList())
		{
			EquipOptionData data3 = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data3 != null)
			{
				int slotIndex = optionInfo.SlotIndex;
				int optionValueStep = data3.OptionValueStep;
				int num = Mathf.Min(optionInfo.OptionStep, optionValueStep);
				OperationType optionType = data3.OptionType;
				int optionValue = data3.GetOptionValue(base.SelectedEquipItemInfo.Star, num);
				int optionValueMax = data3.GetOptionValueMax(base.SelectedEquipItemInfo.Star);
				int selectedOptionSlotIndex = slotIndex;
				Action onClickSelect = delegate
				{
					OnSelectOption(selectedOptionSlotIndex);
				};
				if (data2.RenewableOptionCount == 1 && base.SelectedEquipItemInfo.GetRenewedOptionCount() > 0 && !optionInfo.IsRenewedOption)
				{
					onClickSelect = null;
				}
				_statGaugeComponent.AddStat(optionType, optionValue, optionValueMax, num, optionValueStep, "/", onClickSelect);
			}
		}
	}

	private void OnSelectOption(int optionSlotIndex)
	{
		if (!SingletonUpdater<TutorialManager>.instance.IsProgressing && _listener != null)
		{
			_listener.RenewEquipOption(this, base.SelectedEquipItemInfo.SlotIndex, optionSlotIndex);
		}
	}

	private void CreateRenewableOption()
	{
		if (base.SelectedEquipItemInfo == null)
		{
			ClearRenewableOption();
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.SelectedEquipItemInfo.ItemIndex);
		if (data == null)
		{
			return;
		}
		ClearRenewableOption();
		List<int> allOptionIndex = base.SelectedEquipItemInfo.GetAllOptionIndex(openedOnly: false);
		HashSet<OperationType> hashSet = new HashSet<OperationType>();
		foreach (int item in allOptionIndex)
		{
			EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(item);
			if (data2 != null)
			{
				hashSet.Add(data2.OptionType);
			}
		}
		int[] optionGroupIndex = data.SubData.OptionGroupIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(optionGroupIndex); i++)
		{
			int key = optionGroupIndex[i];
			EquipOptionGroupData data3 = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(key);
			if (data3 == null)
			{
				continue;
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(data3.OptionIndex); j++)
			{
				int key2 = data3.OptionIndex[j];
				EquipOptionData data4 = GameRoot.Instance.EquipOptionDataContainer.GetData(key2);
				if (data4 != null && !hashSet.Contains(data4.OptionType))
				{
					int optionValueMin = data4.GetOptionValueMin(base.SelectedEquipItemInfo.Star);
					int optionValueMax = data4.GetOptionValueMax(base.SelectedEquipItemInfo.Star);
					string optionDisplay = StatData.GetOptionDisplay(data4.OptionType, optionValueMin, optionValueMax, "~");
					SingleStatComponent singleStatComponent = RenewableOptionGrid.AddGridItem(SingleStatComponent.PrefabAuto);
					singleStatComponent.OpenNotAppliedEquipOption(optionDisplay);
					singleStatComponent.SetStatColor(RenewableOptionColor);
					_renewableOptionList.Add(singleStatComponent);
					SprkReforgeUntil.AddPoolEntry(data4.OptionType, optionValueMin, optionValueMax);
				}
			}
		}
		SprkReforgeUntil.PoolBuilt(this, base.SelectedEquipItemInfo.SlotIndex);
		RenewableOptionGrid.RepositionGrid();
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

	public override void OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		base.OnChanged(changedInfo);
		if (changedInfo != null && !CanEnhance(changedInfo.EquipItemInfo))
		{
			RemoveInventoryItemComponent(changedInfo.EquipItemInfo);
			InventoryEquipItemGrid.Grid.Reposition();
		}
		if (changedInfo != null && changedInfo.EquipItemInfo == base.SelectedEquipItemInfo)
		{
			Refresh();
		}
	}

	StatData EquipAwakenScene.IDataProvider.GetBeforeStat()
	{
		return null;
	}

	StatData EquipAwakenScene.IDataProvider.GetAfterStat()
	{
		return null;
	}

	int EquipAwakenScene.IDataProvider.GetBeforeStar()
	{
		return base.SelectedEquipItemInfo.Star;
	}

	int EquipAwakenScene.IDataProvider.GetAfterStar()
	{
		return base.SelectedEquipItemInfo.Star;
	}

	EquipItemData EquipAwakenScene.IDataProvider.GetTargetEquipItemData()
	{
		return GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.SelectedEquipItemInfo.ItemIndex)?.SubData;
	}

	EquipItemInfo EquipAwakenScene.IDataProvider.GetTargetEquipItemInfo()
	{
		return base.SelectedEquipItemInfo;
	}

	Color EquipAwakenScene.IDataProvider.GetFieldColor()
	{
		return Color.white;
	}

	Color EquipAwakenScene.IDataProvider.GetBeforeColor()
	{
		return Color.white;
	}

	Color EquipAwakenScene.IDataProvider.GetAfterColor()
	{
		return Color.white;
	}

	public void Tutorial_InitUi()
	{
		if (InventoryEquipToggle.value)
		{
			InventoryEquipToggle.Set(state: false);
			HeroEquipToggle.Set(state: true);
		}
	}

	public void Tutorial_OnClickInvenEquipitemList()
	{
		CreateInventoryEquipList();
		UIHelper.EnableGameObject(HeroEquipRoot, enable: false);
		UIHelper.EnableGameObject(InventoryEquipRoot, enable: true);
	}

	public void Tutorial_OpenRenewConfirmPopup()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.CONFIRM_FIRST_RENEW_OPTION_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.CONFIRM_FIRST_RENEW_OPTION_MESSAGE);
		MessagePopup.Open(text, message);
	}

	public void Tutorial_OpenPricePopup()
	{
		int num = 0;
		PricedMessagePopup.EPriceType priceType = PricedMessagePopup.EPriceType.Gem;
		string text = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_MESSAGE, 1);
		string action = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ACTION);
		PricedMessagePopup.Open(text, message, action, priceType, num, delegate
		{
			base.Close();
		});
	}
}
