using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ProbabilityTabList : MonoBehaviour, ProbabilityTabComponent.IListener, UIScrollItemBehaviour.IListener
{
	public UIReuseScrollGrid ScrollGrid;

	private List<ProbabilityTabComponent> _component;

	private ProbabilityTabComponent.IListener _listener;

	private List<string> _tabNameList;

	void ProbabilityTabComponent.IListener.OnClickTab(int index)
	{
		_listener.OnClickTab(index);
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		ProbabilityTabComponent probabilityTabComponent = comp as ProbabilityTabComponent;
		if (!(probabilityTabComponent == null))
		{
			probabilityTabComponent.Open(index, _tabNameList[index], this);
		}
	}

	public void Open(ref List<ProbabilityPopup.ProbabilityInfoData> infoList, ProbabilityTabComponent.IListener listener)
	{
		_listener = listener;
		_tabNameList = new List<string>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infoList); i++)
		{
			_tabNameList.Add(infoList[i].Title);
		}
	}

	public void Open(ref List<string> tabNameList, ProbabilityTabComponent.IListener listener)
	{
		_listener = listener;
		_tabNameList = new List<string>();
		_tabNameList.Clear();
		_tabNameList = tabNameList;
	}

	public void CreateTabList()
	{
		ScrollGrid.CreateReuseItem(ProbabilityTabComponent.PrefabAuto, _component, __Extension_Method_ICollection__.COUNT(_tabNameList), this);
		ScrollGrid.RepositionGrid();
	}

	public void ClearTab()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_component);
		ScrollGrid.ClearGridItem<ProbabilityTabComponent>();
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_tabNameList);
	}
}
