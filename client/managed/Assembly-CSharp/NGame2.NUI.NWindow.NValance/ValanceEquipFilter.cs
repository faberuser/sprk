using System;
using NGame2.NUI.NComponent2.Nvalance;
using NShared;

namespace NGame2.NUI.NWindow.NValance;

[Serializable]
public class ValanceEquipFilter : BaseValanceFilterPanel<ValanceEquipFilterImpl>
{
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

	public override void OnClickEquipFilter(ValanceFilterComponent comp, IValanceEquipFilterInfo info)
	{
		if (!(comp == null || info == null))
		{
			bool selected = _filterImpl.Select(info) != EValanceFilterSelected.UnSelect;
			comp.SetSelected(selected);
		}
	}

	public override void OnClickCreatureFilter(ValanceFilterComponent comp, IValanceCreatureFilterInfo info)
	{
	}

	public bool Filter(IValanceEquipFilterInfo info)
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
