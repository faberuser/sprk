using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class WebPayShopPurchaseProtocolHandler : MessageProtocolHandler<WebPayShopPurchase>
{
	public override void Run(long senderId, WebPayShopPurchase protocolData)
	{
		if (protocolData != null)
		{
			SingletonAuto<AttendanceDataContainer>.instance.SetAttendanceData(protocolData.AttendanceDatas);
			GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Apply(protocolData.PlayerProductPurchase);
			GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(protocolData.Attendance);
			GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(protocolData.ItemTimeDurationInfo);
		}
	}
}
