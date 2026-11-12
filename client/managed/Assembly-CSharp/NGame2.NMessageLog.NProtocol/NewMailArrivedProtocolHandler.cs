using NGame2.NUI.NManager;
using NShared.NMessage.NProtocol;

namespace NGame2.NMessageLog.NProtocol;

public class NewMailArrivedProtocolHandler : MessageProtocolHandler<NewMailArrived>
{
	public override void Run(long senderId, NewMailArrived protocolData)
	{
		if (protocolData.IsGlobal)
		{
			MailManagement.IsNewGlobalMailArrived = true;
			SprkMailUpdates.Arrived();
		}
		else
		{
			MailManagement.IsNewMailArrived = true;
			SprkMailUpdates.Arrived();
		}
	}
}
