using NGame2.NDropBonusEvent;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class DropBonusEventAddedProtocolHandler : MessageProtocolHandler<DropBonusEventAdded>
{
	public override void Run(long senderId, DropBonusEventAdded protocolData)
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.Add(protocolData);
	}
}
