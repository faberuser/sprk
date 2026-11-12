using NGame2.NDropBonusEvent;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class DropBonusEventRemovedProtocolHandler : MessageProtocolHandler<DropBonusEventRemoved>
{
	public override void Run(long senderId, DropBonusEventRemoved protocolData)
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.Remove(protocolData.Id);
	}
}
