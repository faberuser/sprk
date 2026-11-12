using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NBattle;

public class WaitPartyMemberPopup : LabelPopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Battle/WaitPartyMemberPopup";

	private static GlobalWindowPrefab<WaitPartyMemberPopup> _windowPrefab;

	public static WaitPartyMemberPopup Open(string message)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new GlobalWindowPrefab<WaitPartyMemberPopup>("Resources/Prefab2/Window/Battle/WaitPartyMemberPopup", GlobalUIRoot.ELayerType.GlobalUI);
		}
		_windowPrefab.instance.OpenInternal(message, closeOnTouch: false);
		GlobalAutoBehaviour<PopupManegement>.instance.AddGlobal(_windowPrefab);
		return _windowPrefab.instance;
	}
}
