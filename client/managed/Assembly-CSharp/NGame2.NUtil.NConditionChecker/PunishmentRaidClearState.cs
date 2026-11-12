using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class PunishmentRaidClearState : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2) || !bool.TryParse(values[3], out var result3))
		{
			return false;
		}
		if (GameRoot.Instance.RaidDataContainer.GetData(result, result2) == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager == null)
		{
			return false;
		}
		PunishmentRaidData data = GameRoot.Instance.PunishmentRaidDataContainer.GetData(result, result2);
		if (data == null)
		{
			return false;
		}
		int groupIndex = data.GroupIndex;
		PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(groupIndex, result, result2);
		return (punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime)) == result3;
	}
}
