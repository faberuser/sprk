using System;
using NVespa;

namespace NGame2.NMessage.NFastHandler;

public class GenericReceiver<T> : IReceiver where T : IMessage
{
	private IVerifier _verifier;

	public Action<T> OnMessage { get; private set; }

	public GenericReceiver(IVerifier verifier, Action<T> onMessage)
	{
		_verifier = verifier;
		OnMessage = onMessage;
	}

	public void Close()
	{
		_verifier = null;
		OnMessage = null;
	}

	internal bool IsValid()
	{
		if (_verifier == null)
		{
			return false;
		}
		if (!_verifier.IsValid())
		{
			return false;
		}
		if (OnMessage == null)
		{
			return false;
		}
		return true;
	}
}
