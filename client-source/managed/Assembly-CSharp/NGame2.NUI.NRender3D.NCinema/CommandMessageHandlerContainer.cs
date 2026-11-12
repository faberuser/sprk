using System;
using System.Collections.Generic;
using NGame2.NMessage;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class CommandMessageHandlerContainer
{
	private Dictionary<string, Action<Cinema, CommandMessage>> _dic = new Dictionary<string, Action<Cinema, CommandMessage>>();

	public void RegistHandlers(List<ICommandMessageHandler> list)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
			{
				ICommandMessageHandler handler = list[i];
				RegistHandler(handler);
			}
		}
	}

	public void RegistHandler(ICommandMessageHandler handler)
	{
		if (handler != null && !String_Extension_Method.IsNullOrEmpty(handler.Command) && handler.Action != null)
		{
			if (_dic.ContainsKey(handler.Command))
			{
				Dictionary<string, Action<Cinema, CommandMessage>> dic = _dic;
				string command = handler.Command;
				dic[command] = (Action<Cinema, CommandMessage>)Delegate.Combine(dic[command], handler.Action);
			}
			else
			{
				_dic.Add(handler.Command, handler.Action);
			}
		}
	}

	public void MessageExecute(Cinema cinema, CommandMessage message)
	{
		if (!(cinema == null) && message.IsValidMessage() && message.IsValidScene(cinema))
		{
			Action<Cinema, CommandMessage> value = null;
			if (_dic.TryGetValue(message.Command, out value))
			{
				Vespa_System_Action_Extension_Method.fire(value, cinema, message);
			}
		}
	}

	public void Close()
	{
		_dic.Clear();
	}

	public void DrawGUI()
	{
		using (new BlockManager().TitleBox("CommandMessageHandler").Vertical())
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_dic))
			{
				GUILayout.Label("Handler IS Empty");
				return;
			}
			int num = 1;
			foreach (KeyValuePair<string, Action<Cinema, CommandMessage>> item in _dic)
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label($"{num}. Command : {item.Key.ToString()}");
				}
				num++;
			}
		}
	}
}
