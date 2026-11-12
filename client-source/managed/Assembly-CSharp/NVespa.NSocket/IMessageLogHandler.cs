using NShared.NMessage;

namespace NVespa.NSocket;

public interface IMessageLogHandler
{
	void Handle(MessageNot messageNot);
}
