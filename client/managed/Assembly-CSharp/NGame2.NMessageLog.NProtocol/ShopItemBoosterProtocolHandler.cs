using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class ShopItemBoosterProtocolHandler : MessageProtocolHandler<ShopItemBooster>
{
	public override void Run(long senderId, ShopItemBooster protocolData)
	{
		if (protocolData != null)
		{
			SingletonAuto<AttendanceDataContainer>.instance.SetAttendanceData(protocolData.AttendanceDatas);
			GlobalAutoBehaviour<AccountManager>.instance.SetPlayerPurchaseTimeDurationInfo(protocolData.PlayerPurchaseTimeDuration);
			GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(protocolData.Attendance);
			GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(protocolData.ItemTimeDurationInfo);
		}
	}
}
