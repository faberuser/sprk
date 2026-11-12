using NGame2.NContents;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUtil;

public class BattleThreadDebugger : GlobalAutoBehaviour<BattleThreadDebugger>
{
	private bool isEnhancedContent;

	private static LocalStorageBool _enableForceEnhanceMode = new LocalStorageBool(i_default: false, typeof(BattleThreadDebugger).FullName + "_enableForceEnhanceMode");

	private string[] contents;

	public bool EnableForceEnhanceMode
	{
		get
		{
			return _enableForceEnhanceMode.Value;
		}
		set
		{
			_enableForceEnhanceMode.Value = value;
		}
	}

	public bool IsEnhancedContent
	{
		get
		{
			return isEnhancedContent;
		}
		set
		{
			isEnhancedContent = value;
		}
	}

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("BattleThreadDebugger", this, DrawControlPanel);
	}

	private void DrawControlPanel()
	{
		using (new BlockManager().Vertical())
		{
			_enableForceEnhanceMode.Value = GUILayout.Toggle(_enableForceEnhanceMode.Value, "Force Enable Enhnace Mode");
			GUILayout.Space(20f);
			if (GUILayout.Button("Update Contents List"))
			{
				contents = SingletonAuto<ContentsValueManager>.instance.GetContentsStringArray("EnhanceModeContentList");
			}
			GUILayout.Space(20f);
			GUILayout.Label("[Update only battle] Enhance Mode: " + isEnhancedContent);
			GUILayout.Space(20f);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(contents))
			{
				GUILayout.Label("Contents List");
				string[] array = contents;
				for (int i = 0; i < array.Length; i++)
				{
					GUILayout.Label(array[i]);
				}
			}
		}
	}
}
