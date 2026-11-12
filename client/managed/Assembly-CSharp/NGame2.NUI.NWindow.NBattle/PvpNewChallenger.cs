using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpNewChallenger : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpNewChallenger";

	public new static PvpNewChallenger Open()
	{
		WindowPrefab<PvpNewChallenger> windowPrefab = new WindowPrefab<PvpNewChallenger>("Resources/Prefab2/Window/Battle/PvpNewChallenger", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	private void OpenInternal()
	{
		base.Open();
	}
}
