using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class TreasureStatViewer
{
	public interface IDataProvider
	{
		int GetMainSlotIndex();

		MonoBehaviour GetMonoBehaviour();

		List<EquipSlotInfo> GetHeroEquippedTreasureList();

		UIPanel GetPanel();

		UIScrollView GetScrollView();
	}

	private IDataProvider _iDataProvider;

	private StatListComponent _statListComponent;

	public TreasureStatViewer(IDataProvider dataProvider)
	{
		_iDataProvider = dataProvider;
	}

	public void Clear()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	public void SetupStat()
	{
		if (_iDataProvider == null)
		{
			return;
		}
		List<EquipSlotInfo> heroEquippedTreasureList = _iDataProvider.GetHeroEquippedTreasureList();
		if (heroEquippedTreasureList == null)
		{
			return;
		}
		Clear();
		int mainSlotIndex = _iDataProvider.GetMainSlotIndex();
		MonoBehaviour monoBehaviour = _iDataProvider.GetMonoBehaviour();
		UIPanel panel = _iDataProvider.GetPanel();
		UIScrollView scrollView = _iDataProvider.GetScrollView();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroEquippedTreasureList); i++)
		{
			EquipSlotInfo equipSlotInfo = heroEquippedTreasureList[i];
			if (equipSlotInfo == null || equipSlotInfo.EquipItemInfo == null)
			{
				continue;
			}
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipSlotInfo.EquipItemInfo.ItemIndex);
			if (data == null || data.SubData == null)
			{
				continue;
			}
			EquipSlotData data2 = GameRoot.Instance.EquipSlotDataContainer.GetData(equipSlotInfo.EquipSlotIndex);
			if (data2 == null)
			{
				continue;
			}
			if (_statListComponent == null)
			{
				_statListComponent = StatListComponent.Create(panel);
			}
			if (equipSlotInfo.EquipSlotIndex == mainSlotIndex)
			{
				flag = true;
				StatListComponent.AddBiggerSubject(_statListComponent, LocalizationManager.GetStringDirect("TREASURE_EQUIP_POPUP_MAIN_OPTION"));
			}
			else if (!flag2)
			{
				if (flag)
				{
					StatListComponent.AddSpacing(_statListComponent);
				}
				flag2 = true;
				StatListComponent.AddBiggerSubject(_statListComponent, LocalizationManager.GetStringDirect("TREASURE_EQUIP_POPUP_SUB_OPTION"));
			}
			if (!data2.IgnoreFixedOption)
			{
				StatListComponent.AddFixedOptionStat(_statListComponent, data.SubData, equipSlotInfo.EquipItemInfo);
			}
			if (!data2.IgnoreRandomOption)
			{
				if (equipSlotInfo.EquipSlotIndex == mainSlotIndex)
				{
					StatListComponent.AddRandomOptionStat(_statListComponent, data.SubData, equipSlotInfo.EquipItemInfo);
				}
				else
				{
					StatListComponent.AddItemIconAndRandomOptionStat(_statListComponent, equipSlotInfo.EquipItemInfo);
				}
			}
		}
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(monoBehaviour, coResetPositionNextFrame(scrollView));
	}

	private IEnumerator coResetPositionNextFrame(UIScrollView scrollView)
	{
		yield return null;
		if (scrollView != null)
		{
			scrollView.ResetPosition();
		}
	}
}
