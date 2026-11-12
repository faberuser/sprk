using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.Nvalance;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NValance;

[Serializable]
public class ValanceCraftFilterPanel : BaseValanceFilterPanel<ValanceCraftFilterImpl>
{
	public enum ECenterMessageType
	{
		None,
		SelectSetType,
		ChangeSetType,
		SelectOther,
		AllSelect
	}

	public interface IListener
	{
		void OnClickEquipFilter(ValanceCraftDataKey selectedKey, int[] equipOptionIndices);
	}

	private IListener _listener;

	public ValanceCraftDataKey SelectedKey => _filterImpl.SelectedKey;

	public int SelectDisplayEquipOptionIdex => _filterImpl.SelectDisplayEquipOptionIndex;

	public void SetListener(IListener listener)
	{
		_listener = listener;
	}

	public override void SetFilterGroupComponent()
	{
		if (FilterScrollView == null || FilterTable == null)
		{
			return;
		}
		IList<ValanceFilterGroupData> dataListByGroupType = GameRoot.Instance.ValanceFilterGroupDataContainer.GetDataListByGroupType(EValanceFilterGroupType.TechnoMagic);
		for (int i = 0; i < dataListByGroupType.Count; i++)
		{
			ValanceFilterGroupData valanceFilterGroupData = dataListByGroupType[i];
			if (valanceFilterGroupData != null)
			{
				ValanceFilterGroupComponent valanceFilterGroupComponent = ValanceFilterGroupComponent.PrefabAuto.Factory.Create(FilterTable.transform);
				valanceFilterGroupComponent.Open(valanceFilterGroupData, this);
				if (i > 0)
				{
					valanceFilterGroupComponent.SetEnable(isEnable: false);
				}
				else
				{
					valanceFilterGroupComponent.SetEnable(isEnable: true);
				}
				_listGroupComp.Add(valanceFilterGroupComponent);
			}
		}
		IList<ValanceFilterGroupData> dataListByGroupType2 = GameRoot.Instance.ValanceFilterGroupDataContainer.GetDataListByGroupType(EValanceFilterGroupType.Craft);
		for (int j = 0; j < dataListByGroupType2.Count; j++)
		{
			ValanceFilterGroupData valanceFilterGroupData2 = dataListByGroupType2[j];
			if (valanceFilterGroupData2 != null)
			{
				ValanceFilterGroupComponent valanceFilterGroupComponent2 = ValanceFilterGroupComponent.PrefabAuto.Factory.Create(FilterTable.transform);
				valanceFilterGroupComponent2.Open(valanceFilterGroupData2, this);
				valanceFilterGroupComponent2.SetEnable(isEnable: false);
				_listGroupComp.Add(valanceFilterGroupComponent2);
			}
		}
		FilterTable.Reposition();
		FilterScrollView.ResetPosition();
	}

	public override void Reset()
	{
		base.Reset();
		SetSubGroupEnable();
	}

	public override void OnClickCreatureFilter(ValanceFilterComponent comp, IValanceCreatureFilterInfo info)
	{
	}

	private CreatureTagType GetCreatureTagType(IValanceEquipFilterInfo info)
	{
		if (info.IsCreatureTagTypeArray)
		{
			return info.CreatureTagTypes[0];
		}
		return info.CreatureTagType;
	}

	public override void OnClickEquipFilter(ValanceFilterComponent comp, IValanceEquipFilterInfo info)
	{
		ECenterMessageType messageType = ECenterMessageType.None;
		if (SelectedKey.SetType == EquipItemSetGroupType.None)
		{
			messageType = ECenterMessageType.SelectSetType;
		}
		else if (SelectedKey.EquipPartType == EquipPartType.None && SelectedKey.EquipItemSubType == EquipItemSubType.None && info.EquipPartType != EquipPartType.None)
		{
			messageType = (((SelectedKey.EquipPartType == info.EquipPartType && SelectedKey.EquipItemSubType == info.EquipItemSubType) || SelectedKey.CreatureTagType == CreatureTagType.None) ? ECenterMessageType.SelectOther : ECenterMessageType.AllSelect);
		}
		else if (SelectedKey.CreatureTagType == CreatureTagType.None && GetCreatureTagType(info) != CreatureTagType.None)
		{
			messageType = ((SelectedKey.EquipPartType == EquipPartType.None) ? ECenterMessageType.SelectOther : ECenterMessageType.AllSelect);
		}
		if (info.SetType != EquipItemSetGroupType.None)
		{
			Reset();
		}
		EValanceFilterSelected num = _filterImpl.Select(info);
		CenterMessage(messageType);
		bool isSelected = num != EValanceFilterSelected.UnSelect;
		SetFilterSelected(comp, isSelected);
		SetSubGroupEnable();
		NoticeOwnerWindow();
	}

	private void CenterMessage(ECenterMessageType messageType)
	{
		if (messageType != ECenterMessageType.None)
		{
			string key = "";
			switch (messageType)
			{
			case ECenterMessageType.ChangeSetType:
				key = "VALANCE_CRAFT_SELECT_CHANGESETTYPE";
				break;
			case ECenterMessageType.SelectOther:
				key = "VALANCE_CRAFT_SELECT_SELECTOTHER";
				break;
			case ECenterMessageType.SelectSetType:
				key = "VALANCE_CRAFT_SELECT_SELECTSETTYPE";
				break;
			case ECenterMessageType.AllSelect:
				key = "VALANCE_CRAFT_SELECT_ADDSKILL";
				break;
			}
			NGame2.NUI.NWindow.CenterMessage.Open(LocalizationManager.GetStringDirect(key));
		}
	}

	private void SetFilterSelected(ValanceFilterComponent comp)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ValanceFilterGroupComponent valanceFilterGroupComponent = _listGroupComp[i];
			if (valanceFilterGroupComponent == null)
			{
				continue;
			}
			valanceFilterGroupComponent.ForFilterComponent(delegate(ValanceFilterComponent filterComp)
			{
				if (filterComp.FilterType == comp.FilterType)
				{
					bool selected = comp == filterComp;
					filterComp.SetSelected(selected);
				}
			});
		}
	}

	private void SetFilterSelected(ValanceFilterComponent comp, bool isSelected)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ValanceFilterGroupComponent valanceFilterGroupComponent = _listGroupComp[i];
			if (valanceFilterGroupComponent == null)
			{
				continue;
			}
			valanceFilterGroupComponent.ForFilterComponent(delegate(ValanceFilterComponent filterComp)
			{
				if (filterComp.FilterType == comp.FilterType)
				{
					bool selected = comp == filterComp && isSelected;
					filterComp.SetSelected(selected);
				}
			});
		}
	}

	public void SetNotice()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ValanceFilterGroupComponent valanceFilterGroupComponent = _listGroupComp[i];
			if (valanceFilterGroupComponent == null)
			{
				continue;
			}
			valanceFilterGroupComponent.ForFilterComponent(delegate(ValanceFilterComponent filterComp)
			{
				if (!(filterComp == null) && filterComp.FilterType == EValanceFilterSelected.SetType)
				{
					ValanceEquipFilterData key = filterComp.GetFilterInfo() as ValanceEquipFilterData;
					ValanceCraftData data = GameRoot.Instance.ValanceCraftDataContainer.GetData(key);
					if (data != null && data.NoticeCheck)
					{
						int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(data.MaterialItemIndex);
						int materialItemCount = data.MaterialItemCount;
						bool isShow = itemCount >= materialItemCount;
						filterComp.ShowNotice(isShow);
					}
				}
			});
		}
	}

	private void SetSubGroupEnable()
	{
		bool isEnableSub = _filterImpl.SelectedKey.SetType != EquipItemSetGroupType.None;
		bool isEnableExtraSkill = _filterImpl.IsAllSelected();
		for (int i = 1; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ValanceFilterGroupComponent valanceFilterGroupComponent = _listGroupComp[i];
			if (valanceFilterGroupComponent == null)
			{
				continue;
			}
			valanceFilterGroupComponent.ForFilterComponent(delegate(ValanceFilterComponent comp)
			{
				if (comp.FilterType != EValanceFilterSelected.ExtraEquipOptionIndex)
				{
					comp.SetEnable(isEnableSub);
				}
				else
				{
					comp.SetEnable(isEnableExtraSkill);
				}
			});
		}
	}

	private void NoticeOwnerWindow()
	{
		if (_listener != null)
		{
			_listener.OnClickEquipFilter(_filterImpl.SelectedKey, _filterImpl.EquipOptionIndices);
		}
	}

	public override void OnClickFoldOut(bool isFoldOut)
	{
		if (FilterTable != null)
		{
			FilterTable.Reposition();
		}
		if (FilterScrollView != null)
		{
			FilterScrollView.ResetPosition();
		}
	}
}
