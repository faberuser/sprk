using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public static class UnityLogQueue
{
	public struct LogData
	{
		public string condition;

		public LogType type;
	}

	private static Dictionary<LogType, Queue<(string log, string stackTrace)>> _queues_by_LogType = new Dictionary<LogType, Queue<(string, string)>>();

	private static Queue<string> _queue_for_IAP = new Queue<string>();

	private static GUIStyle __logLabelStyle;

	private static GUIStyle _logLabelStyle
	{
		get
		{
			if (__logLabelStyle == null)
			{
				__logLabelStyle = VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).WordWrap().RichText()
					.Style;
			}
			return __logLabelStyle;
		}
	}

	public static void Enqueue_IAP()
	{
		lock (_queue_for_IAP)
		{
			_queue_for_IAP.Enqueue(string.Empty);
		}
	}

	public static void Enqueue_IAP_Seperator(string str = null)
	{
		lock (_queue_for_IAP)
		{
			if (str == null)
			{
				_queue_for_IAP.Enqueue("=============================================");
			}
			else
			{
				_queue_for_IAP.Enqueue(str);
			}
		}
	}

	public static void Enqueue_IAP(string log, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNumber = 0)
	{
		lock (_queue_for_IAP)
		{
			if (!String_Extension_Method.IsNullOrEmpty(log) && lineNumber > 0 && !String_Extension_Method.IsNullOrEmpty(callerFilePath))
			{
				int num = callerFilePath.IndexOf("unity_project");
				if (num > 0)
				{
					callerFilePath = callerFilePath.Substring(num);
				}
				log = $"{log} : {callerFilePath}({lineNumber})";
			}
			_queue_for_IAP.Enqueue(log);
		}
	}

	public static void Clear_IAP()
	{
		lock (_queue_for_IAP)
		{
			_queue_for_IAP.Clear();
		}
	}

	public static void Enqueue(string condition, string stackTrace, LogType type)
	{
		lock (_queues_by_LogType)
		{
			if (!_queues_by_LogType.TryGetValue(type, out Queue<(string, string)> value))
			{
				value = new Queue<(string, string)>();
				_queues_by_LogType.Add(type, value);
			}
			value.Enqueue((condition, stackTrace));
			if (value.Count > 100)
			{
				value.Dequeue();
			}
		}
	}

	public static void SimpleLookupGui_Error()
	{
		SimpleLookupGui(LogType.Error);
	}

	public static void SimpleLookupGui_Assert()
	{
		SimpleLookupGui(LogType.Assert);
	}

	public static void SimpleLookupGui_Warning()
	{
		SimpleLookupGui(LogType.Warning);
	}

	public static void SimpleLookupGui_Log()
	{
		SimpleLookupGui(LogType.Log);
	}

	public static void SimpleLookupGui_Exception()
	{
		SimpleLookupGui(LogType.Exception);
	}

	private static void SimpleLookupGui(LogType logType)
	{
		lock (_queues_by_LogType)
		{
			if (!_queues_by_LogType.TryGetValue(logType, out Queue<(string, string)> value))
			{
				return;
			}
			float num = GlobalAutoBehaviour<GuiManager>.instance.DisplayWidth - 150f;
			float num2 = GlobalAutoBehaviour<GuiManager>.instance.DisplayHeight - 50f;
			using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(num - 50f), GUILayout.Height(num2 - 100f)).Scroll(value, "Log", alwaysShowHorizontal: false, alwaysShowVertical: true))
			{
				foreach (var (text, text2) in value)
				{
					GUILayout.Label(text + "\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n" + text2 + "\n---------------------------------------\n\n", _logLabelStyle, GUILayout.Width(num - 90f));
				}
			}
		}
	}

	public static void SimpleLookupGui_IAP()
	{
		lock (_queue_for_IAP)
		{
			float num = GlobalAutoBehaviour<GuiManager>.instance.DisplayWidth - 150f;
			float num2 = GlobalAutoBehaviour<GuiManager>.instance.DisplayHeight - 50f;
			using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(num - 50f), GUILayout.Height(num2 - 100f)).Scroll(_queue_for_IAP, "Log", alwaysShowHorizontal: false, alwaysShowVertical: true))
			{
				foreach (string item in _queue_for_IAP)
				{
					GUILayout.Label(item, _logLabelStyle, GUILayout.Width(num - 90f));
				}
			}
			if (GUILayout.Button("Clear", GUILayout.Width(200f), GUILayout.Height(40f)))
			{
				_queue_for_IAP.Clear();
			}
		}
	}
}
