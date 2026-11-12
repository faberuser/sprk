using System;
using NGame2.NMessage;

namespace NGame2.NUI.NRender3D.NCinema;

public interface ICommandMessageHandler
{
	string Command { get; }

	Action<Cinema, CommandMessage> Action { get; }
}
