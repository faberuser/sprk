using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NEventForge;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

public class EventForgePercentageInfoPopup : BaseWindow, UIScrollItemBehaviour.IListener, ProbabilityTabComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/EventForge/EventForgeProbabilityPopup";

	public UIReuseScrollGrid PercentageInfoReuseScrollGrid;

	public ProbabilityTabList TabList;

	private List<EventForgeData> _eventForgeDataList;

	private List<EventForgePercentageInfoComponent> _eventForgePercentageInfoCompList = new List<EventForgePercentageInfoComponent>();

	public new void Open()
	{
		Open(delegate
		{
			Init();
		}, null);
	}

	private void Init()
	{
		InitTab();
		SetEventForgePercentageInfoList(1);
	}

	public override void Close()
	{
		base.Close();
		ReleaseEventForgePercentageInfoCompList();
		TabList.ClearTab();
	}

	private void InitTab()
	{
		IList<ItemData> itemDataListByType = GameRoot.Instance.ItemDataContainer.GetItemDataListByType(ItemType.EventEnchantWeapon);
		if (itemDataListByType == null)
		{
			return;
		}
		List<string> tabNameList = new List<string>();
		tabNameList.Clear();
		foreach (ItemData item in itemDataListByType)
		{
			if (item != null)
			{
				tabNameList.Add(item.Name);
			}
		}
		TabList.Open(ref tabNameList, this);
		TabList.CreateTabList();
	}

	private void ReleaseEventForgePercentageInfoCompList()
	{
		FactoryBase.ReleaseAndClearList(_eventForgePercentageInfoCompList);
		if (PercentageInfoReuseScrollGrid != null)
		{
			PercentageInfoReuseScrollGrid.ClearGridItem<EventForgePercentageInfoComponent>();
		}
	}

	private void SetEventForgePercentageInfoList(int group)
	{
		ReleaseEventForgePercentageInfoCompList();
		if (PercentageInfoReuseScrollGrid == null)
		{
			return;
		}
		_eventForgeDataList = GameRoot.Instance.EventForgeDataContainer.GetDataListByGroup(group);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_eventForgeDataList))
		{
			_eventForgeDataList.Sort((EventForgeData data, EventForgeData compData) => data.EnchantLevel.CompareTo(compData.EnchantLevel));
			PercentageInfoReuseScrollGrid.CreateReuseItem(EventForgePercentageInfoComponent.PrefabAuto, _eventForgePercentageInfoCompList, _eventForgeDataList.Count, this);
			UIHelper.RepositionGrid(PercentageInfoReuseScrollGrid);
		}
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		EventForgePercentageInfoComponent eventForgePercentageInfoComponent = comp as EventForgePercentageInfoComponent;
		if (eventForgePercentageInfoComponent != null && !__Extension_Method_ICollection__.IsNullOrEmpty(_eventForgeDataList) && _eventForgeDataList.Count > index)
		{
			EventForgeData eventForgeData = _eventForgeDataList[index];
			if (eventForgeData != null)
			{
				eventForgePercentageInfoComponent.Open(eventForgeData);
			}
		}
	}

	void ProbabilityTabComponent.IListener.OnClickTab(int index)
	{
		SetEventForgePercentageInfoList(index + 1);
	}
}
