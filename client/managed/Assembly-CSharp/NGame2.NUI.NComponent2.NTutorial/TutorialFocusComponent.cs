using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NTutorial;

public class TutorialFocusComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Tutorial/TutorialFocusComponent";

	public static PrefabAuto<TutorialFocusComponent> PrefabAuto = new PrefabAuto<TutorialFocusComponent>("Prefab2/Component/Tutorial/TutorialFocusComponent");

	public UIWidget OutlineWidget;

	public int MinX { get; private set; }

	public int MinY { get; private set; }

	public int MaxX { get; private set; }

	public int MaxY { get; private set; }

	public bool Open()
	{
		UIHelper.EnableGameObject(OutlineWidget, enable: false);
		return true;
	}

	public void SetFocusOutline(int minX, int minY, int maxX, int maxY)
	{
		MinX = minX;
		MinY = minY;
		MaxX = maxX;
		MaxY = maxY;
		if ((bool)OutlineWidget)
		{
			UIHelper.EnableGameObject(OutlineWidget, enable: true);
			OutlineWidget.SetRect(minX, minY, maxX - minX, maxY - minY);
			OutlineWidget.ForceUpdateAnchors(includeChildren: true);
		}
	}
}
