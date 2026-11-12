using NGame2.NPayShopItemEvent;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class PayShopItemEventRemovedProtocolHandler : MessageProtocolHandler<PayShopItemEventRemoved>
{
	public override void Run(long senderId, PayShopItemEventRemoved protocolData)
	{
		if (protocolData != null)
		{
			GlobalAutoBehaviour<PayShopItemEventManager>.instance.Remove(protocolData.Id);
		}
	}
}
