using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial;

public class MouseCursorManager : GlobalAutoBehaviour<MouseCursorManager>
{
	private string _output;

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("MouseCursor", this, DrawGui);
	}

	private void Update()
	{
		GlobalUIRoot globalUIRoot;
		if (Application.isPlaying)
		{
			if (!GlobalUIRoot.isValidInstance)
			{
				return;
			}
			globalUIRoot = GlobalUIRoot.instance;
		}
		else
		{
			globalUIRoot = Object.FindObjectOfType<GlobalUIRoot>();
			if (globalUIRoot == null)
			{
				return;
			}
		}
		Camera camera = globalUIRoot.GetLayerInfo(GlobalUIRoot.ELayerType.GlobalUI).Camera;
		Vector3 mousePosition = VespaInput.mousePosition;
		Vector3 vector = Vespa_Unity_Camera_Extension_Method.VespaScreenToWorldPoint(camera, mousePosition) * globalUIRoot.UIRoot.activeHeight / 2f;
		_output = "POS = (" + (int)vector.x + ", " + (int)vector.y + ")";
	}

	private void DrawGui()
	{
		GUILayout.Label(_output);
	}
}
