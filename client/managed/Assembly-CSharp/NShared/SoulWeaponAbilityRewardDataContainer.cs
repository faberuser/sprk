using System.Collections;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class SoulWeaponAbilityRewardDataContainer : JitDataContainerDefault<SoulWeaponAbilityRewardKey, SoulWeaponAbilityRewardData>
{
	public class ItemCodeAndCount_for_JsonRead
	{
		public string ItemCode { get; set; }

		public int ItemCount { get; set; }
	}

	public const string TABLENAME = "SoulWeaponAbilityRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponAbilityRewardTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, SoulWeaponAbilityRewardData value)
	{
		value.ItemCodeList = new List<string>();
		value.ItemCountList = new List<int>();
		for (int i = 1; i <= 5; i++)
		{
			if (jsonDic.Contains("ItemCode" + i))
			{
				ItemCodeAndCount_for_JsonRead itemCodeAndCount_for_JsonRead = JsonMarshalerUsingReflection.Parse<ItemCodeAndCount_for_JsonRead>(jsonDic, string.Empty, i.ToString());
				if (itemCodeAndCount_for_JsonRead != null && !String_Extension_Method.IsNullOrEmpty(itemCodeAndCount_for_JsonRead.ItemCode))
				{
					value.ItemCodeList.Add(itemCodeAndCount_for_JsonRead.ItemCode);
					value.ItemCountList.Add(itemCodeAndCount_for_JsonRead.ItemCount);
				}
			}
		}
	}

	public SoulWeaponAbilityRewardData GetData(ESoulWeaponAbilityTicketType ticketType, CreatureTagType tagType, int grade, int reinforceLevel)
	{
		return GetData(new SoulWeaponAbilityRewardKey(ticketType, tagType, grade, reinforceLevel));
	}
}
