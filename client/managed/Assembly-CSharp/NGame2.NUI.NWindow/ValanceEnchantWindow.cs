using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceEnchantWindow : ValanceEnhanceBase, ValanceEnchantOptionComponent.IListener, ItemIconComponent.Listener, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>
{
	public interface IListener
	{
		void OnClickEnchant(int equipItemSlotIndex, int enchantCoreItemIndex);

		void OnClickConfirmEnchant(int equipItemSlotIndex, bool isNew);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceEnchantWindow";

	[Header("Enchant")]
	public UIButton enchantButton;

	public UILabel noticeSelectItem;

	public Transform[] coreItemRoots;

	public UILabel[] labelCoreItems;

	public UILabel labelRequiredGold;

	private string[] enchantCoreItems = new string[4] { "TECHNOMAGIC_ENCHANT_TRANSISTOR", "TECHNOMAGIC_ENCHANT_PULSE", "TECHNOMAGIC_ENCHANT_NANO", "TECHNOMAGIC_ENCHANT_ENERGY" };

	private List<ItemIconComponent> coreItemIconComponentList = new List<ItemIconComponent>();

	private ItemIconComponent selectedCoreItem;

	[Header("Effect")]
	public GameObject circuitEffect;

	public ValanceEnchantOptionComponent currentOptionComponent;

	public ValanceEnchantOptionComponent newOptionComponent;

	private readonly int requiredCoreAmount = 1;

	public static bool EnchantInProgress;

	private IListener _listener;

	public EquipItemDetailData EquipItemDetailData { get; private set; }

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo != null)
		{
			UpdateEnchantAvailability();
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		UpdateCoreCount();
		UpdateEnchantAvailability();
	}

	public void Open(IListener listener)
	{
		base.Open(delegate
		{
			_listener = listener;
			Init();
		}, CheckLastEnchantResult);
	}

	public void OpenCustomCinema(EquipItemInfo itemInfo, IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceEnchantWindowInven", delegate
		{
			Init(itemInfo);
			ItemListPanel.ExternalClickItemIcon(itemInfo, isRefreshTab: true);
		}, CheckLastEnchantResult);
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
		if (selectedCoreItem != null)
		{
			selectedCoreItem.SetSelected(selected: false);
			selectedCoreItem = null;
		}
		SetEnchantOngoing(isOngoing: false);
		ItemListPanel.DisableAllItemSelect(disable: false);
		currentOptionComponent.Open(isNewOption: false, this);
		newOptionComponent.Open(isNewOption: true, this);
		currentOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
		newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
		for (int i = 0; i < enchantCoreItems.Length; i++)
		{
			if (enchantCoreItems[i] == null || coreItemRoots[i] == null || labelCoreItems[i] == null)
			{
				return;
			}
			string code = enchantCoreItems[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode == null)
			{
				return;
			}
			InitCoreIcon(dataByCode, coreItemRoots[i]);
		}
		OnChangeSelectedItem();
		UpdateCoreCount();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		OpenTechnoEnchant triggerEvent = new OpenTechnoEnchant();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void InitCoreIcon(ItemData materialData, Transform iconRoot)
	{
		ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(iconRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
		itemIconComponent.Open(materialData, this);
		coreItemIconComponentList.Add(itemIconComponent);
	}

	public override void SelectedTargetItem(EquipItemInfo itemInfo)
	{
		if (!EnchantInProgress)
		{
			if (TargetComponent != null && SprkCurrentEnchantTarget(TargetComponent) == itemInfo)
			{
				base.SelectedTargetItem(itemInfo);
				return;
			}
			base.SelectedTargetItem(itemInfo);
			OnChangeSelectedItem();
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent == null || TargetComponent == null)
		{
			return;
		}
		if (itemIconComponent != TargetComponent)
		{
			OnClickCoreItem(itemIconComponent);
		}
		else
		{
			if (EnchantInProgress)
			{
				return;
			}
			OnClickTechnoMagicItem(itemIconComponent);
		}
		OnChangeSelectedItem();
	}

	private void OnClickCoreItem(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.EquipPartType != EquipPartType.None)
		{
			return;
		}
		if (selectedCoreItem == itemIconComponent)
		{
			if (!EnchantInProgress)
			{
				selectedCoreItem.SetSelected(selected: false);
				selectedCoreItem = null;
			}
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemIconComponent.ItemData.Code) > 0)
		{
			if (selectedCoreItem != null)
			{
				selectedCoreItem.SetSelected(selected: false);
			}
			selectedCoreItem = itemIconComponent;
			selectedCoreItem.SetSelected(selected: true);
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("ITEM_NAME_" + selectedCoreItem.ItemData.Index);
			string text = GameRoot.Instance.ValanceEnchantOptionCountDataContainer.GetMinOptionCount(selectedCoreItem.ItemData.Code).ToString();
			string text2 = GameRoot.Instance.ValanceEnchantOptionCountDataContainer.GetMaxOptionCount(selectedCoreItem.ItemData.Code).ToString();
			string stringDirect3 = LocalizationManager.GetStringDirect("COMMON_SPECIALCHARACTER_TILDE");
			string text3 = ((text == text2) ? text2 : new StringBuilder().AppendFormat("{0}{1}{2}", text, stringDirect3, text2).ToString());
			string stringDirect4 = LocalizationManager.GetStringDirect("VALANCE_ENCHANT_CORESELECT_POPUP", stringDirect2, text3);
			MessagePopup.Open(stringDirect, stringDirect4, null);
		}
	}

	private void SetEnchantOngoing(bool isOngoing)
	{
		EnchantInProgress = isOngoing;
	}

	private void CheckLastEnchantResult()
	{
		ValanceEnchantOptionInfo valanceEnchant = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceEnchant();
		if (valanceEnchant == null)
		{
			return;
		}
		int enchantEquipItemSlotIndex = valanceEnchant.EnchantEquipItemSlotIndex;
		if (enchantEquipItemSlotIndex == 0)
		{
			return;
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(enchantEquipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("Failed to find equip item of enchant : equipItemSlotIndex=" + enchantEquipItemSlotIndex);
			return;
		}
		ItemListPanel.ExternalClickItemIcon(equipItemInfo, isRefreshTab: true);
		foreach (ItemIconComponent coreItemIconComponent in coreItemIconComponentList)
		{
			if (coreItemIconComponent.ItemData.Index == valanceEnchant.ConsumeItemIndex)
			{
				selectedCoreItem = coreItemIconComponent;
				selectedCoreItem.SetSelected(selected: true);
			}
		}
		UpdateEnchantAvailability();
		SetEnchantOngoing(isOngoing: true);
		OnEnchantResponse(valanceEnchant);
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
		string stringDirect2 = LocalizationManager.GetStringDirect("VALANCE_ENCHANT_INCOMPLETE_POPUP");
		MessagePopup.Open(stringDirect, stringDirect2, null);
	}

	public void OnClickEnchant()
	{
		if (!(TargetComponent == null) && !(selectedCoreItem == null))
		{
			UIHelper.EnableButton(enchantButton, enable: false, grayscaleIfDisable: true);
			currentOptionComponent.DisableConfirmButton();
			newOptionComponent.DisableConfirmButton();
			if (EnchantInProgress)
			{
				_listener.OnClickConfirmEnchant(SprkCurrentEnchantTarget(TargetComponent).SlotIndex, isNew: false);
			}
			SetEnchantOngoing(isOngoing: true);
			if (!SprkCurrentEnchantTarget(TargetComponent).EnchantOptionExists())
			{
				SetEnchantOngoing(isOngoing: false);
			}
			if (_listener != null)
			{
				_listener.OnClickEnchant(SprkCurrentEnchantTarget(TargetComponent).SlotIndex, selectedCoreItem.ItemData.Index);
			}
		}
	}

	void ValanceEnchantOptionComponent.IListener.OnClickEnchantConfirm(bool isNew)
	{
		if (!EnchantInProgress)
		{
			return;
		}
		SetEnchantOngoing(isOngoing: false);
		currentOptionComponent.HideConfirmButton();
		newOptionComponent.HideConfirmButton();
		if (_listener != null && TargetComponent != null)
		{
			_listener.OnClickConfirmEnchant(SprkCurrentEnchantTarget(TargetComponent).SlotIndex, isNew);
			if (isNew)
			{
				currentOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Out);
				newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Out);
			}
			else
			{
				newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Out);
			}
		}
	}

	private void UpdateCoreCount()
	{
		for (int i = 0; i < enchantCoreItems.Length && !(coreItemIconComponentList[i] == null) && !(labelCoreItems[i] == null); i++)
		{
			ItemIconComponent itemIconComponent = coreItemIconComponentList[i];
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemIconComponent.ItemData.Code);
			string text = $"{itemCount} / {requiredCoreAmount}";
			UIHelper.SetLabelText(labelCoreItems[i], text);
			Color color = ((requiredCoreAmount <= itemCount) ? Color.white : Color.red);
			UIHelper.SetLabelColor(labelCoreItems[i], color);
		}
	}

	private void OnChangeSelectedItem()
	{
		if (TargetComponent == null)
		{
			currentOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
			newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
		}
		else if (!EnchantInProgress)
		{
			EquipItemInfo info = SprkCurrentEnchantTarget(TargetComponent);
			if (selectedCoreItem == null)
			{
				currentOptionComponent.ShowCurrentOptionList(info);
				newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
			}
			else
			{
				string code = selectedCoreItem.ItemData.Code;
				int minOptionCount = GameRoot.Instance.ValanceEnchantOptionCountDataContainer.GetMinOptionCount(code);
				int maxOptionCount = GameRoot.Instance.ValanceEnchantOptionCountDataContainer.GetMaxOptionCount(code);
				ValanceEnchantOptionComponent valanceEnchantOptionComponent = currentOptionComponent;
				if (SprkCurrentEnchantTarget(TargetComponent).EnchantOptionExists())
				{
					currentOptionComponent.ShowCurrentOptionList(info);
					valanceEnchantOptionComponent = newOptionComponent;
				}
				else
				{
					newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
				}
				valanceEnchantOptionComponent.ShowOptionChanceList(minOptionCount, maxOptionCount);
				valanceEnchantOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.ShowChance);
			}
		}
		UIHelper.EnableGameObject(noticeSelectItem, TargetComponent == null);
		EnableCircuitEffectObject(selectedCoreItem != null && TargetComponent != null);
		UpdateEnchantAvailability();
	}

	private void UpdateEnchantAvailability()
	{
		if (IsReadyToEnchant())
		{
			UIHelper.EnableButton(enchantButton, enable: true, grayscaleIfDisable: true);
		}
		else
		{
			UIHelper.EnableButton(enchantButton, enable: false, grayscaleIfDisable: true);
		}
	}

	private bool IsReadyToEnchant()
	{
		if (!CheckIfGoldEnough())
		{
			return false;
		}
		if (TargetComponent == null)
		{
			return false;
		}
		if (selectedCoreItem == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(selectedCoreItem.ItemData.Code) <= 0)
		{
			return false;
		}
		return true;
	}

	private bool CheckIfGoldEnough()
	{
		if (TargetComponent == null || selectedCoreItem == null)
		{
			SetRequiredGoldLabel(0);
			return false;
		}
		string code = selectedCoreItem.ItemData.Code;
		string partType = TargetComponent.EquipItemData.PartType.ToString();
		string subType = TargetComponent.EquipItemData.SubType.ToString();
		int reqGold = GameRoot.Instance.ValanceEnchantDataContainer.GetReqGold(code, partType, subType);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold < reqGold)
		{
			SetRequiredGoldLabel(reqGold, isEnough: false);
			return false;
		}
		SetRequiredGoldLabel(reqGold);
		return true;
	}

	private void SetRequiredGoldLabel(int gold, bool isEnough = true)
	{
		UIHelper.SetLabelText(labelRequiredGold, Vespa_Int_Extension_Method.ToCommaString(gold));
		UIHelper.SetLabelColor(labelRequiredGold, isEnough ? Color.white : Color.red);
	}

	public void OnEnchantResponse(ValanceEnchantOptionInfo newInfo)
	{
		if (!EnchantInProgress)
		{
			currentOptionComponent.ShowCurrentOptionList(SprkCurrentEnchantTarget(TargetComponent), skipAnimation: false);
			currentOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.NewOption);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceEnchant(null);
			Refresh();
		}
		else
		{
			ItemListPanel.DisableAllItemSelect(disable: true);
			currentOptionComponent.ShowConfirmButton();
			newOptionComponent.ShowNewOptionList(newInfo);
		}
	}

	public void OnEnchantConfirmResponse()
	{
		if (!EnchantInProgress)
		{
			currentOptionComponent.ShowCurrentOptionList(SprkCurrentEnchantTarget(TargetComponent), skipAnimation: false);
			Refresh();
		}
	}

	public override void OnClickComfirmCreatureFilter()
	{
		base.OnClickComfirmCreatureFilter();
		ItemListPanel.CheckIfItemIconDisabled();
	}

	public override void OnClickComfirmItemFilter()
	{
		base.OnClickComfirmItemFilter();
		ItemListPanel.CheckIfItemIconDisabled();
	}

	public override void Refresh()
	{
		base.Refresh();
		ItemListPanel.DisableAllItemSelect(disable: false);
		if (selectedCoreItem != null)
		{
			selectedCoreItem.SetSelected(selected: false);
			selectedCoreItem = null;
		}
		if (!(TargetComponent == null))
		{
			ItemListPanel.ExternalClickItemIcon(SprkCurrentEnchantTarget(TargetComponent));
			UpdateEnchantAvailability();
		}
	}

	public override void ClearSeletedTarget()
	{
		base.ClearSeletedTarget();
		OnChangeSelectedItem();
		UIHelper.EnableGameObject(noticeSelectItem, enable: true);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ItemListPanel.DisableAllItemSelect(disable: false);
		SetEnchantOngoing(isOngoing: false);
		newOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
		currentOptionComponent.SetPanelState(ValanceEnchantOptionComponent.EnchantPanelState.Hide);
		FactoryBase.ReleaseAndClearList(coreItemIconComponentList);
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public override void OnSystemBackButton()
	{
		ValanceEnchantWindow window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<ValanceEnchantWindow>();
		BaseWindow topOpenedWindow = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopOpenedWindow();
		if (EnchantInProgress && topOpenedWindow == window)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("VALANCE_ENCHANT_BACKBUTTON_POPUP");
			MessagePopup.Open(stringDirect, stringDirect2, null);
		}
		else
		{
			Close();
		}
	}

	private void EnableCircuitEffectObject(bool enable)
	{
		UIHelper.EnableGameObject(circuitEffect, enable);
	}

	private static EquipItemInfo SprkCurrentEnchantTarget(ItemIconComponent P_0)
	{
		EquipItemInfo equipItemInfo = P_0.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemInfo.SlotIndex) ?? equipItemInfo;
	}
}
