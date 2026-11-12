using System;
using System.Text;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NGlobal.NGuiManager;

public static class GameConsole
{
	private class OutputContainer
	{
		private bool _isDirty;

		private string _output = string.Empty;

		private StringBuilder _builder = new StringBuilder();

		public void Append(string output)
		{
			if (String_Extension_Method.IsNullOrEmpty(output))
			{
				return;
			}
			lock (_sync)
			{
				_isDirty = true;
				_builder.AppendLine();
				_builder.Append(output);
				if (_builder.Length > MaxLength)
				{
					int length = _builder.Length - MaxLength;
					_builder.Remove(0, length);
				}
			}
		}

		public void Clear()
		{
			lock (_sync)
			{
				_builder.Remove(0, _builder.Length);
				_isDirty = true;
			}
		}

		public string GetOutputString(out bool isDirty)
		{
			if (!_isDirty)
			{
				isDirty = false;
				return _output;
			}
			lock (_sync)
			{
				isDirty = _isDirty;
				if (_isDirty)
				{
					_output = _builder.ToString();
					_isDirty = false;
				}
				return _output;
			}
		}
	}

	private static object _sync = new object();

	private static bool _initialized;

	public static Action<string> InputListen;

	private static string _input = string.Empty;

	private static int MaxLength = 10000;

	private static OutputContainer _outputContainer = new OutputContainer();

	private static LocalStorageBool _autoScroll = new LocalStorageBool(i_default: true, "Global_GuiManager_GameConsole_AutoScroll");

	public static void InitConsole()
	{
		if (!_initialized)
		{
			GlobalAutoBehaviour<GuiManager>.instance.Add("Console", _sync, DrawGui);
			_outputContainer.Append("[Output]");
			_initialized = true;
		}
	}

	public static void LogCallback(string condition, LogType type)
	{
		if (_initialized && type == LogType.Error)
		{
			string output = Vespa_String_Extension_Method.ColorTag("[E]" + condition, Color.red);
			_outputContainer.Append(output);
		}
	}

	private static void DrawGui()
	{
		bool isDirty;
		string outputString = _outputContainer.GetOutputString(out isDirty);
		if (isDirty && _autoScroll.Value)
		{
			ScrollBlock.SetScroll(_sync, "Output", Vector2.up * 1000000f);
		}
		float num = GlobalAutoBehaviour<GuiManager>.instance.DisplayWidth - 150f;
		float num2 = GlobalAutoBehaviour<GuiManager>.instance.DisplayHeight - 50f;
		using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(num - 50f), GUILayout.Height(num2 - 100f)).Scroll(_sync, "Output", alwaysShowHorizontal: false, alwaysShowVertical: true))
		{
			GUILayout.Label(outputString, VespaGuiStyleBuilder_Extension_Method.Builder(GUI.skin.label).WordWrap().RichText()
				.Style, GUILayout.Width(num - 90f));
			}
			using (new BlockManager().Horizontal())
			{
				bool flag = false;
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "GameConsole_Input")
				{
					flag = true;
				}
				if (GUILayout.Button("Enter", GUILayout.Width(60f)))
				{
					flag = true;
				}
				if (flag)
				{
					_outputContainer.Append("> " + _input);
					Vespa_System_Action_Extension_Method.fire(InputListen, _input);
					_input = string.Empty;
					return;
				}
				GUI.SetNextControlName("GameConsole_Input");
				_input = GUILayout.TextField(_input, GUILayout.Width(num - 210f));
				if (GUILayout.Button("Clear", GUILayout.Width(50f)))
				{
					_outputContainer.Clear();
				}
			}
		}
	}
