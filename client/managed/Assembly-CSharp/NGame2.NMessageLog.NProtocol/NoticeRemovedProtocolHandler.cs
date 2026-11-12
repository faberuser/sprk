using NGame2.NNotice;
using NShared.NMessage.NProtocol;
using NVespa;

namespace NGame2.NMessageLog.NProtocol;

public class NoticeRemovedProtocolHandler : MessageProtocolHandler<NoticeRemoved>
{
	public override void Run(long senderId, NoticeRemoved protocolData)
	{
		if (protocolData != null)
		{
			SingletonAuto<NoticeManager>.instance.RemoveNotice(protocolData.Id);
		}
	}
}
