using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;

namespace NGame2.NMessageLog.NProtocol;

public class GmPopupMessageArrivedProtocolHandler : MessageProtocolHandler<GmPopupMessageArrived>
{
	public override void Run(long senderId, GmPopupMessageArrived protocolData)
	{
		if (protocolData != null)
		{
			string title = protocolData.Title;
			string content = protocolData.Content;
			MessagePopup.Open(title, content);
		}
	}
}
