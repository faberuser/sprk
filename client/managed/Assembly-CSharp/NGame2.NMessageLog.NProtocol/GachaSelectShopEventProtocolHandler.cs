using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GachaSelectShopEventProtocolHandler : MessageProtocolHandler<GachaSelectShopEvent>
{
	public override void Run(long senderId, GachaSelectShopEvent protocolData)
	{
		if (AccountManager.IsLogin() && GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager != null && protocolData != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.GachaSelectShopManager.ApplySelectShopNoticeInfo(protocolData.SelectShopNoticeInfos);
		}
	}
}
