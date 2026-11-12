using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.Nvalance;
using NShared;

namespace NGame2.NUI.NWindow.NValance;

[Serializable]
public class ValanceCreatureFilter : BaseValanceFilterPanel<ValanceCreatureFilterImpl>
{
	public override void SetFilterGroupComponent()
	{
		if (FilterScrollView == null || FilterTable == null)
		{
			return;
		}
		IList<ValanceFilterGroupData> dataListByGroupType = GameRoot.Instance.ValanceFilterGroupDataContainer.GetDataListByGroupType(EValanceFilterGroupType.Creature);
		for (int i = 0; i < dataListByGroupType.Count; i++)
		{
			ValanceFilterGroupData valanceFilterGroupData = dataListByGroupType[i];
			if (valanceFilterGroupData != null)
			{
				ValanceFilterGroupComponent valanceFilterGroupComponent = ValanceFilterGroupComponent.PrefabAuto.Factory.Create(FilterTable.transform);
				valanceFilterGroupComponent.Open(valanceFilterGroupData, this);
				_listGroupComp.Add(valanceFilterGroupComponent);
			}
		}
		FilterTable.Reposition();
		FilterScrollView.ResetPosition();
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

	public override void OnClickCreatureFilter(ValanceFilterComponent comp, IValanceCreatureFilterInfo info)
	{
		if (!(comp == null || info == null))
		{
			bool selected = _filterImpl.Select(info) != EValanceFilterSelected.UnSelect;
			comp.SetSelected(selected);
		}
	}

	public override void OnClickEquipFilter(ValanceFilterComponent comp, IValanceEquipFilterInfo info)
	{
	}

	public bool Filter(IValanceCreatureFilterInfo info)
	{
		if (_filterImpl.IsNoneSelected())
		{
			return true;
		}
		return _filterImpl.Filter(info);
	}

	public bool IsNoneSelected()
	{
		return _filterImpl.IsNoneSelected();
	}
}
