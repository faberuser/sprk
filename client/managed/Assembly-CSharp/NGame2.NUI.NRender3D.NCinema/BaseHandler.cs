using System;
using NGame2.NMessage;

namespace NGame2.NUI.NRender3D.NCinema;

public abstract class BaseHandler : ICommandMessageHandler
{
	public abstract string Command { get; }

	Action<Cinema, CommandMessage> ICommandMessageHandler.Action => Action;

	public abstract void Action(Cinema owner, CommandMessage message);
}
