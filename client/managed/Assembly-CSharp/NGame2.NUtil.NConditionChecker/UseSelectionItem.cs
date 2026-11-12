using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class UseSelectionItem : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2))
		{
			return false;
		}
		PlayerItemUseInfo playerItemUseInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemUseManager.Get(result);
		if (playerItemUseInfo != null && playerItemUseInfo.OutItemIndex == result2)
		{
			return true;
		}
		return false;
	}
}
