using System;
using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecommendDeckInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => AccountId;

	public string AccountId { get; set; }

	public RecommendDeckType Type { get; set; }

	public int Rank { get; set; }

	public int PlayTime { get; set; }

	public ClearPartyHeroInfo[] Heroes { get; set; }

	public EquipItemInfo[] EquipItems { get; set; }

	public static CachedHeroInfo CreateCachedHeroInfo(ClearPartyHeroInfo heroInfo, EquipItemInfo[] equipItems)
	{
		if (heroInfo == null)
		{
			return null;
		}
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			int equipSlotIndex = heroInfo.GetEquipItemSlotIndex(allEquipSlotDatum.Index);
			if (equipSlotIndex != 0)
			{
				EquipItemInfo equipItemInfo = Array.Find(equipItems, (EquipItemInfo item) => item.SlotIndex == equipSlotIndex);
				if (equipItemInfo != null)
				{
					list.Add(equipItemInfo);
				}
			}
		}
		return new CachedHeroInfo(heroInfo, list, null, heroInfo.PunishmentRuneOptionInfos);
	}
}
