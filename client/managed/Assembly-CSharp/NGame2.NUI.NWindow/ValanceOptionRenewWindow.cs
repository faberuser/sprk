using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceOptionRenewWindow : ValanceEnhanceBase, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public interface IListener
	{
		void OnClickRenew(int equipItemSlotIndex, int optionSlotIndex);

		void OpenLastRenewResult(int equipItemSlotIndex, int optionSlotIndex);

		void OnClickSkillRenew(int equipItemSlotIndex, int skillSlotIndex);

		void OpenLastSkillRenewResult(int equipItemSlotIndex, int skillSlotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceOptionRenewWindow";

	public UITable EquipOptionTable;

	public UIWidget MaterialWidget;

	public Transform MaterialItemRoot1;

	public Transform MaterialItemRoot2;

	public UILabel LabelReqMaterialCount1;

	public UILabel LabelReqMaterialCount2;

	public UILabel LabelEmptyTarget;

	public UILabel LabelEmptyMaterial;

	public UIButton Button_Renew;

	private IListener _listener;

	private StatGaugeComponent _statGaugeComponent;

	private StatGaugeComponent _extraSkillComponent;

	private ItemIconComponent _materialComp1;

	private ItemIconComponent _materialComp2;

	private int _selectOptionIndex;

	private int _selectSkillIndex;

	public EquipItemDetailData EquipItemDetailData { get; private set; }

	private bool IsRenewOption => _selectOptionIndex != 0;

	private bool IsRenewSkill => _selectSkillIndex != 0;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && !(TargetComponent == null) && EquipItemDetailData != null && (changedInfo.ItemIndex == EquipItemDetailData.RenewItemIndex1 || changedInfo.ItemIndex == EquipItemDetailData.RenewItemIndex2 || changedInfo.ItemIndex == EquipItemDetailData.ExtraSkillRenewItemIndex))
		{
			SetupMaterialIcon();
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
			if (equipItemInfo != null && equipItemInfo.IsTechnoMagic && equipItemInfo.IsIdentified)
			{
				SetInvenItemList();
			}
		}
		else if (changedInfo.EquipItemInfo == base.EquipItemInfo)
		{
			Refresh();
		}
	}

	public void Open(IListener listener)
	{
		base.Open(delegate
		{
			_listener = listener;
			Init();
		}, CheckLastRenewResult);
	}

	public void OpenCustomCinema(EquipItemInfo itemInfo, IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceOptionRenewWindowInven", delegate
		{
			Init(itemInfo);
			ItemListPanel.ExternalClickItemIcon(itemInfo, isRefreshTab: true);
		}, CheckLastRenewResult);
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
		SetTargetFrame();
		SetRenewButton();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	private void CheckLastRenewResult()
	{
		ValanceRenewOptionInfo valanceOptionRenew = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceOptionRenew();
		if (valanceOptionRenew != null && valanceOptionRenew.RenewOptionEquipItemSlotIndex != 0)
		{
			OpenLastOptionResult(valanceOptionRenew);
			return;
		}
		ValanceRenewSkillInfo valanceSkillRenew = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceSkillRenew();
		if (valanceSkillRenew != null && valanceSkillRenew.RenewSkillEquipItemSlotIndex != 0)
		{
			OpenLastSkillResult(valanceSkillRenew);
		}
	}

	private void OpenLastOptionResult(ValanceRenewOptionInfo renewOptionInfo)
	{
		int renewOptionEquipItemSlotIndex = renewOptionInfo.RenewOptionEquipItemSlotIndex;
		if (renewOptionEquipItemSlotIndex == 0)
		{
			return;
		}
		int renewOptionResultIndex = renewOptionInfo.RenewOptionResultIndex;
		if (renewOptionResultIndex == 0)
		{
			return;
		}
		int renewOptionSlotIndex = renewOptionInfo.RenewOptionSlotIndex;
		if (renewOptionSlotIndex != 0)
		{
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(renewOptionEquipItemSlotIndex);
			if (equipItemInfo == null)
			{
				VespaDebug.LogError("Failed to find equip item of renew : equipItemSlotIndex=" + renewOptionEquipItemSlotIndex);
			}
			else if (equipItemInfo.GetRenewedOptionCount() == 0)
			{
				VespaDebug.LogError("RenewOptionSlotIndex = 0 : equipItemSlotIndex=" + renewOptionEquipItemSlotIndex + " renewOptionResultIndex=" + renewOptionResultIndex);
			}
			else if (_listener != null)
			{
				_listener.OpenLastRenewResult(renewOptionEquipItemSlotIndex, renewOptionSlotIndex);
			}
		}
	}

	private void OpenLastSkillResult(ValanceRenewSkillInfo renewSkillInfo)
	{
		int renewSkillEquipItemSlotIndex = renewSkillInfo.RenewSkillEquipItemSlotIndex;
		if (renewSkillEquipItemSlotIndex == 0)
		{
			return;
		}
		int renewSkillResultIndex = renewSkillInfo.RenewSkillResultIndex;
		if (renewSkillResultIndex == 0)
		{
			return;
		}
		int renewSkillSlotIndex = renewSkillInfo.RenewSkillSlotIndex;
		if (renewSkillSlotIndex != 0)
		{
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(renewSkillEquipItemSlotIndex);
			if (equipItemInfo == null)
			{
				VespaDebug.LogError("Failed to find equip item of renew : equipItemSlotIndex=" + renewSkillEquipItemSlotIndex);
			}
			else if (equipItemInfo.GetRenewedSkillCount() == 0)
			{
				VespaDebug.LogError("RenewOptionSlotIndex = 0 : equipItemSlotIndex=" + renewSkillEquipItemSlotIndex + " renewOptionResultIndex=" + renewSkillResultIndex);
			}
			else if (_listener != null)
			{
				_listener.OpenLastSkillRenewResult(renewSkillEquipItemSlotIndex, renewSkillSlotIndex);
			}
		}
	}

	private void SetTargetFrame()
	{
		bool flag = TargetComponent != null;
		UIHelper.EnableGameObject(LabelEmptyTarget, !flag);
		UIHelper.EnableGameObject(MaterialWidget, flag);
		bool flag2 = IsRenewOption || IsRenewSkill;
		UIHelper.EnableGameObject(LabelEmptyMaterial, flag && !flag2);
	}

	public void SetRenewButton()
	{
		bool flag = TargetComponent != null;
		bool flag2 = IsRenewOption || IsRenewSkill;
		UIHelper.EnableButton(Button_Renew, flag && flag2);
	}

	public override void SelectedTargetItem(EquipItemInfo itemInfo)
	{
		base.SelectedTargetItem(itemInfo);
		_selectOptionIndex = 0;
		_selectSkillIndex = 0;
		ClearMaterialIcon();
		CreateStatGauge();
		CreateExtraSkill();
		RepositionTable();
		RefreshStatComp();
		RefreshExtraSkill();
		SetTargetFrame();
		SetRenewButton();
	}

	private void CreateStatGauge()
	{
		if (base.EquipItemInfo == null)
		{
			ClearStatGauge();
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.EquipItemInfo.ItemIndex);
		if (data == null)
		{
			ClearStatGauge();
			VespaDebug.LogError("EquipItemData not found : index=" + base.EquipItemInfo.ItemIndex);
			return;
		}
		EquipItemDetailData = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (EquipItemDetailData == null)
		{
			ClearStatGauge();
			VespaDebug.LogError("EquipItemDetailData not found : index=" + data.SubData.DetailIndex);
			return;
		}
		ClearStatGauge();
		_statGaugeComponent = StatGaugeComponent.ValanceButtonPrefabAuto.Factory.Create(EquipOptionTable.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_statGaugeComponent.transform);
		string stringDirect = LocalizationManager.GetStringDirect("RANDOM_OPTION");
		_statGaugeComponent.SetTitle(stringDirect);
		foreach (EquipItemOptionInfo optionInfo in base.EquipItemInfo.GetOptionInfoList())
		{
			EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data2 != null)
			{
				int optionValueStep = data2.OptionValueStep;
				int num = Mathf.Min(optionInfo.OptionStep, optionValueStep);
				OperationType optionType = data2.OptionType;
				int optionValue = data2.GetOptionValue(base.EquipItemInfo.Star, num);
				int optionValueMax = data2.GetOptionValueMax(base.EquipItemInfo.Star);
				int slotIndex = optionInfo.SlotIndex;
				Action onClickSelect = delegate
				{
					OnSelectOption(slotIndex);
				};
				_statGaugeComponent.AddStat(optionType, optionValue, optionValueMax, num, optionValueStep, "/", onClickSelect);
			}
		}
	}

	private void CreateExtraSkill()
	{
		if (base.EquipItemInfo == null)
		{
			ClearExtraSkill();
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.EquipItemInfo.ItemIndex);
		if (data == null)
		{
			ClearExtraSkill();
			VespaDebug.LogError("EquipItemData not found : index=" + base.EquipItemInfo.ItemIndex);
			return;
		}
		EquipItemDetailData = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (EquipItemDetailData == null)
		{
			ClearExtraSkill();
			VespaDebug.LogError("EquipItemDetailData not found : index=" + data.SubData.DetailIndex);
			return;
		}
		if (!base.EquipItemInfo.IsIdentified)
		{
			ClearExtraSkill();
			return;
		}
		ClearExtraSkill();
		_extraSkillComponent = StatGaugeComponent.ValanceSkillButtonPrefabAuto.Factory.Create(EquipOptionTable.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_extraSkillComponent.transform);
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_OPTION_ADDITIONALSKILL");
		_extraSkillComponent.SetTitle(stringDirect);
		List<EquipItemOptionInfo> extraOptionInfoList = base.EquipItemInfo.GetExtraOptionInfoList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraOptionInfoList); i++)
		{
			EquipItemOptionInfo equipItemOptionInfo = extraOptionInfoList[i];
			if (equipItemOptionInfo == null)
			{
				continue;
			}
			EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(equipItemOptionInfo.OptionIndex);
			if (data2 == null)
			{
				VespaDebug.LogError("Option data not found : " + equipItemOptionInfo.OptionIndex);
				continue;
			}
			int optionValueStep = data2.OptionValueStep;
			int currentStep = Mathf.Min(equipItemOptionInfo.OptionStep, optionValueStep);
			int optionValue = data2.GetOptionValue(base.EquipItemInfo.Star, currentStep);
			string optionDisplay = StatData.GetOptionDisplay(data2.OptionType, optionValue, opened: true, 0, appendColor: false);
			int slotIndex = equipItemOptionInfo.SlotIndex;
			Action onClickSelect = delegate
			{
				OnSelectSkill(slotIndex);
			};
			_extraSkillComponent.AddExtraSkill(optionDisplay, onClickSelect);
		}
	}

	private void SetupMaterialIcon()
	{
		if (EquipItemDetailData == null)
		{
			return;
		}
		ClearMaterialIcon();
		if (IsRenewOption)
		{
			int renewItemIndex = EquipItemDetailData.RenewItemIndex1;
			int renewItemCount = EquipItemDetailData.RenewItemCount1;
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(renewItemIndex);
			if (data != null)
			{
				SetMaterialIcon(data, renewItemCount, MaterialItemRoot1, LabelReqMaterialCount1, ref _materialComp1);
			}
			int renewItemIndex2 = EquipItemDetailData.RenewItemIndex2;
			int renewItemCount2 = EquipItemDetailData.RenewItemCount2;
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(renewItemIndex2);
			if (data2 != null)
			{
				SetMaterialIcon(data2, renewItemCount2, MaterialItemRoot2, LabelReqMaterialCount2, ref _materialComp2);
			}
		}
		else
		{
			int extraSkillRenewItemIndex = EquipItemDetailData.ExtraSkillRenewItemIndex;
			int extraSkillRenewItemCount = EquipItemDetailData.ExtraSkillRenewItemCount;
			ItemData data3 = GameRoot.Instance.ItemDataContainer.GetData(extraSkillRenewItemIndex);
			if (data3 != null)
			{
				SetMaterialIcon(data3, extraSkillRenewItemCount, MaterialItemRoot1, LabelReqMaterialCount1, ref _materialComp1);
			}
		}
	}

	private void SetMaterialIcon(ItemData materialData, int materialCount, Transform iconRoot, UILabel label, ref ItemIconComponent comp)
	{
		if (comp == null)
		{
			comp = ItemIconComponent.PrefabAuto.Factory.Create(iconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(comp.transform);
		}
		comp.Open(materialData);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialData.Code);
		Color color = ((materialCount <= itemCount) ? Color.white : Color.red);
		string text = $"{itemCount} / {materialCount}";
		UIHelper.SetLabelColor(label, color);
		UIHelper.SetLabelText(label, text);
	}

	private void RefreshStatComp()
	{
		if (_statGaugeComponent == null)
		{
			return;
		}
		int num = 0;
		while (true)
		{
			StatGaugeInfo statGaugeInfo = _statGaugeComponent.GetStatGaugeInfo(num);
			if (!(statGaugeInfo == null))
			{
				int num2 = num + 1;
				if (base.EquipItemInfo == null || base.EquipItemData == null)
				{
					statGaugeInfo.SelectRoot(StatGaugeInfo.StateType.Disabled);
				}
				else
				{
					StatGaugeInfo.StateType stateType = StatGaugeInfo.StateType.None;
					stateType = ((_selectOptionIndex == num2) ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.None);
					statGaugeInfo.SelectRoot(stateType);
				}
				num++;
				continue;
			}
			break;
		}
	}

	public void RefreshExtraSkill()
	{
		if (_extraSkillComponent == null)
		{
			return;
		}
		int num = 0;
		while (true)
		{
			StatGaugeInfo statGaugeInfo = _extraSkillComponent.GetStatGaugeInfo(num);
			if (!(statGaugeInfo == null))
			{
				int num2 = num + 1;
				if (base.EquipItemInfo == null || base.EquipItemData == null)
				{
					statGaugeInfo.SelectRoot(StatGaugeInfo.StateType.Disabled);
				}
				else
				{
					StatGaugeInfo.StateType stateType = StatGaugeInfo.StateType.None;
					stateType = ((_selectSkillIndex == num2) ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.None);
					statGaugeInfo.SelectRoot(stateType);
				}
				num++;
				continue;
			}
			break;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!(TargetComponent == null))
		{
			bool isRenewOption = IsRenewOption;
			int optionSlotIndex = (IsRenewOption ? _selectOptionIndex : _selectSkillIndex);
			ItemListPanel.ExternalClickItemIcon(TargetComponent.EquipItemInfo);
			if (isRenewOption)
			{
				OnSelectOption(optionSlotIndex);
			}
			else
			{
				OnSelectSkill(optionSlotIndex);
			}
		}
	}

	private void RepositionTable()
	{
		if ((bool)EquipOptionTable)
		{
			EquipOptionTable.Reposition();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public override void ClearSeletedTarget()
	{
		base.ClearSeletedTarget();
		_selectOptionIndex = 0;
		_selectSkillIndex = 0;
		ClearStatGauge();
		ClearExtraSkill();
		ClearMaterialIcon();
		SetTargetFrame();
		SetRenewButton();
		RepositionTable();
	}

	private void ClearStatGauge()
	{
		FactoryBase.SafeReleaseComponent(ref _statGaugeComponent);
	}

	private void ClearExtraSkill()
	{
		FactoryBase.SafeReleaseComponent(ref _extraSkillComponent);
	}

	private void ClearMaterialIcon()
	{
		if (_materialComp1 != null)
		{
			UIHelper.SetLabelText(LabelReqMaterialCount1, "");
			FactoryBase.ReleaseComponent(_materialComp1);
			_materialComp1 = null;
		}
		if (_materialComp2 != null)
		{
			UIHelper.SetLabelText(LabelReqMaterialCount2, "");
			FactoryBase.ReleaseComponent(_materialComp2);
			_materialComp2 = null;
		}
	}

	private bool IsEnoughMaterial()
	{
		if (EquipItemDetailData == null)
		{
			return false;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(EquipItemDetailData.RenewItemIndex1);
		bool num = EquipItemDetailData.RenewItemCount1 <= itemCount;
		int itemCount2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(EquipItemDetailData.RenewItemIndex2);
		bool flag = EquipItemDetailData.RenewItemCount2 <= itemCount2;
		return num && flag;
	}

	private void OnSelectOption(int optionSlotIndex)
	{
		_selectOptionIndex = optionSlotIndex;
		_selectSkillIndex = 0;
		OnSelectStatComp();
	}

	private void OnSelectSkill(int optionSlotIndex)
	{
		_selectSkillIndex = optionSlotIndex;
		_selectOptionIndex = 0;
		OnSelectStatComp();
	}

	private void OnSelectStatComp()
	{
		RefreshStatComp();
		RefreshExtraSkill();
		SetupMaterialIcon();
		SetTargetFrame();
		SetRenewButton();
	}

	public void OnClickRenew()
	{
		if (!IsRenewOption && !IsRenewSkill)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_OPTION_RENEW.ASK_RENEW_OPTION_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("VALANCE_RENEW_CONFIRM_POPUP");
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			if (_listener != null)
			{
				if (IsRenewOption)
				{
					_listener.OnClickRenew(base.EquipItemInfo.SlotIndex, _selectOptionIndex);
				}
				else
				{
					_listener.OnClickSkillRenew(base.EquipItemInfo.SlotIndex, _selectSkillIndex);
				}
			}
		}, null);
	}
}
