using NGame2.NChatting;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class ServerShutdownProtocolHandler : MessageProtocolHandler<ServerShutdown>
{
	public override void Run(long senderId, ServerShutdown protocolData)
	{
		if (protocolData != null)
		{
			string text = null;
			text = ((protocolData.Second < 60) ? LocalizationManager.GetString(MessageLog.SERVER_SHUTDOWN_MESSAGE_SECOND, protocolData.Second) : LocalizationManager.GetString(MessageLog.SERVER_SHUTDOWN_MESSAGE_MINUTE, protocolData.Second / 60));
			GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(text, ChatType.Channel, showPreview: true, 0L);
		}
	}
}
