using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class NewbieBuyItemBonusCount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 1);
		NewbieRewardType valueEnum = ArgParser.GetValueEnum<string, NewbieRewardType>((IList<string>)values, 2, NewbieRewardType.None);
		if (valueInt == 0 || valueEnum == NewbieRewardType.None)
		{
			return false;
		}
		NewbieRewardProductData[] dataByRewardType = GameRoot.Instance.NewbieRewardProductDataContainer.GetDataByRewardType(valueEnum);
		if (__Extension_Method_ICollection__.COUNT(dataByRewardType) == 0)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(dataByRewardType); i++)
		{
			NewbieRewardProductData newbieRewardProductData = dataByRewardType[i];
			if (newbieRewardProductData != null && GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(newbieRewardProductData.PayShopProductIndex) != null)
			{
				num++;
			}
		}
		if (num >= valueInt)
		{
			return true;
		}
		return false;
	}
}
