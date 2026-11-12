using NGame2.NPayShopItemEvent;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class PayShopItemEventAddedProtocolHandler : MessageProtocolHandler<PayShopItemEventAdded>
{
	public override void Run(long senderId, PayShopItemEventAdded protocolData)
	{
		if (protocolData != null)
		{
			GlobalAutoBehaviour<PayShopItemEventManager>.instance.Add(protocolData, isListSort: true);
		}
	}
}
