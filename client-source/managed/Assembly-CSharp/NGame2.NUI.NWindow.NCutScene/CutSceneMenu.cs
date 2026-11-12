using NGame2.NCutScene;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NCutScene;

public class CutSceneMenu : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/CutScene/CutSceneMenu";

	public UIWidget RootWidget;

	public UIButton SkipButton;

	public void OpenWindow(bool canSkip)
	{
		base.Open();
		UIHelper.EnableGameObject(SkipButton, canSkip);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickSkip()
	{
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.Skip();
		}
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(RootWidget, show);
	}

	public void ShowSkipButton(bool isShow)
	{
		UIHelper.EnableGameObject(SkipButton, isShow);
	}
}
