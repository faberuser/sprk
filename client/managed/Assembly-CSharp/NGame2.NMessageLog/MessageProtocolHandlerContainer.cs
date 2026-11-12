using System;
using System.Collections.Generic;
using NShared.NMessage;
using NVespa;

namespace NGame2.NMessageLog;

public class MessageProtocolHandlerContainer<T> where T : IMessageNot
{
	private readonly Dictionary<string, IMessageProtocolHandler> _handlerDictionary = new Dictionary<string, IMessageProtocolHandler>();

	private readonly string _handlerName;

	public MessageProtocolHandlerContainer(string handlerName = null)
	{
		_handlerName = handlerName;
	}

	public void RegisterHandler(IMessageProtocolHandler handler)
	{
		string name = handler.GetProtocolType().Name;
		RemoveHandler(name);
		_handlerDictionary.Add(name, handler);
	}

	public bool RemoveHandler(string protocolName)
	{
		return _handlerDictionary.Remove(protocolName);
	}

	public bool RemoveHandler(IMessageProtocolHandler handler)
	{
		string name = handler.GetProtocolType().Name;
		return RemoveHandler(name);
	}

	public void Handle(T messageNot)
	{
		if (messageNot == null)
		{
			return;
		}
		string type = messageNot.Type;
		string content = messageNot.Content;
		try
		{
			IMessageProtocolHandler value = null;
			if (_handlerDictionary.TryGetValue(type, out value))
			{
				value?.Run(messageNot.SenderId, content);
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to process message (" + _handlerName + "." + type + "): error=" + ex?.ToString() + " message=" + messageNot.Content);
		}
	}
}
