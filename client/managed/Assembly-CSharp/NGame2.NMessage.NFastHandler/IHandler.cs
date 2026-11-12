using System;

namespace NGame2.NMessage.NFastHandler;

public interface IHandler
{
	Type HandleType { get; }

	bool Handle(IMessage message);

	void ClearEnd();
}
