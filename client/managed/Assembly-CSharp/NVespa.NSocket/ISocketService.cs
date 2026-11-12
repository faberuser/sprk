using System;

namespace NVespa.NSocket;

public interface ISocketService
{
	Type GetHandleType();

	void Handle(object context, string content);
}
