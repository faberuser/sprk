using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class WebViewManager : GlobalAutoBehaviour<WebViewManager>, InitOnCreate
{
	private string _input = string.Empty;

	void InitOnCreate.InitializeSingleton()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("DebugWebView", this, Draw);
	}

	public void Draw()
	{
		bool flag = false;
		using (new BlockManager().Horizontal())
		{
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "urlInput")
			{
				flag = true;
			}
			GUI.SetNextControlName("urlInput");
			_input = GUILayout.TextField(_input, GUILayout.Width(GlobalAutoBehaviour<GuiManager>.instance.DisplayWidth - 300f));
			if (GUILayout.Button("Enter", GUILayout.Width(60f)))
			{
				flag = true;
			}
			if (flag)
			{
				DebugWebViewPopup.OpenGlobal(_input);
				_input = string.Empty;
				return;
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Add http://", GUILayout.Width(130f)))
			{
				_input = "http://";
			}
			if (GUILayout.Button("Add https://", GUILayout.Width(130f)))
			{
				_input = "https://";
			}
			if (GUILayout.Button("Clear", GUILayout.Width(130f)))
			{
				_input = string.Empty;
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Herostat Live", GUILayout.Width(130f)))
			{
				_input = "https://kr-apne2-web.masangsoft.com/stats/v1/html/index.html?lang=ko";
			}
			if (GUILayout.Button("Herostat Dev", GUILayout.Width(130f)))
			{
				_input = "https://kr-apne2-web.masangsoft.com/stats/v1/html/index.html?lang=ko";
			}
			if (GUILayout.Button("Naver Cafe", GUILayout.Width(130f)))
			{
				_input = "https://cafe.naver.com/kingsraid";
			}
		}
	}
}
