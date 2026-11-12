using NGame2.NUI.NManager;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class RecommendOptionPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/RecommendOptionPopup";

	public UIScrollGrid AttackOptionGrid;

	public UIScrollGrid DefenceOptionGrid;

	public new static void Open()
	{
		WindowPrefab<RecommendOptionPopup> windowPrefab = new WindowPrefab<RecommendOptionPopup>("Resources/Prefab2/Window/RecommendOptionPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal()
	{
		base.Open();
	}
}
