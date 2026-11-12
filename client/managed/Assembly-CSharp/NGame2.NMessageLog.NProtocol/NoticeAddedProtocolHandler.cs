using NGame2.NNotice;
using NShared.NMessage.NProtocol;
using NVespa;

namespace NGame2.NMessageLog.NProtocol;

public class NoticeAddedProtocolHandler : MessageProtocolHandler<NoticeAdded>
{
	public override void Run(long senderId, NoticeAdded protocolData)
	{
		if (protocolData != null)
		{
			SingletonAuto<NoticeManager>.instance.AddNotice(protocolData);
		}
	}
}
