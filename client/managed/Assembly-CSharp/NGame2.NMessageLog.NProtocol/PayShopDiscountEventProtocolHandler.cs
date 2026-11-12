using NGame2.NAccount;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class PayShopDiscountEventProtocolHandler : MessageProtocolHandler<PayShopDiscountEvent>
{
	public override void Run(long senderId, PayShopDiscountEvent protocolData)
	{
		if (AccountManager.IsLogin() && GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Apply(protocolData.PayShopDiscountInfos);
		}
	}
}
