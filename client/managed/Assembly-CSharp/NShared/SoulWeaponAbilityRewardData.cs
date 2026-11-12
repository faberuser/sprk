using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponAbilityRewardData : IDataKeySelector<SoulWeaponAbilityRewardKey>
{
	SoulWeaponAbilityRewardKey IDataKeySelector<SoulWeaponAbilityRewardKey>.Key => new SoulWeaponAbilityRewardKey(Type, TagType, Grade, ReinforceLevel);

	[Key(0)]
	public ESoulWeaponAbilityTicketType Type { get; set; }

	[Key(1)]
	public CreatureTagType TagType { get; set; }

	[Key(2)]
	public int Grade { get; set; }

	[Key(3)]
	public int ReinforceLevel { get; set; }

	[Key(4)]
	public List<string> ItemCodeList { get; set; }

	[Key(5)]
	public List<int> ItemCountList { get; set; }

	public IList<string> GetItemCodeList()
	{
		return ItemCodeList;
	}

	public IList<int> GettemCountList()
	{
		return ItemCountList;
	}

	public string GetItemCodeByIndex(int index)
	{
		if (index < ItemCodeList.Count)
		{
			return ItemCodeList[index];
		}
		return null;
	}

	public int GetItemCount(int index)
	{
		if (index < ItemCountList.Count)
		{
			return ItemCountList[index];
		}
		return 0;
	}
}
