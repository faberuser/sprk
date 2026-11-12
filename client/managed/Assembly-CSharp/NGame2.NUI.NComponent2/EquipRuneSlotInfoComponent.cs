using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NHelper;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipRuneSlotInfoComponent : MonoBehaviour
{
	[Serializable]
	public class EquipRuneSlotUIInfo
	{
		public RuneSlotType Slot;

		public UIWidget Widget_On;
	}

	public interface IDataProvider
	{
		ItemData GetItemData();
	}

	[Header("[Rune Slot]")]
	public EquipRuneSlotUIInfo[] EquipRuneSlotUIInfos;

	private IDataProvider _dataProvider;

	public void Open(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		Refresh();
	}

	public void Refresh()
	{
		if (_dataProvider != null)
		{
			ItemData itemData = _dataProvider.GetItemData();
			if (itemData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(EquipRuneSlotUIInfos) && (itemData.Type == ItemType.Rune || itemData.Type == ItemType.PunishmentRune))
			{
				List<RuneSlotType> canEquipRuneSlots = RuneHelper.GetRuneSlotTypes(itemData).ToList();
				DirectRefresh(canEquipRuneSlots);
			}
		}
	}

	public void DirectRefresh(List<RuneSlotType> canEquipRuneSlots)
	{
		EquipRuneSlotUIInfo[] equipRuneSlotUIInfos = EquipRuneSlotUIInfos;
		foreach (EquipRuneSlotUIInfo info in equipRuneSlotUIInfos)
		{
			if (!(info.Widget_On == null))
			{
				RuneSlotType runeSlotType = canEquipRuneSlots.Find((RuneSlotType e) => e == info.Slot);
				UIHelper.EnableGameObject(info.Widget_On, runeSlotType != RuneSlotType.None);
			}
		}
	}
}
