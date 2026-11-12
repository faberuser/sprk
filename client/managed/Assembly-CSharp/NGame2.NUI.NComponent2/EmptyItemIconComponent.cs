using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EmptyItemIconComponent : UIScrollItemBehaviour
{
	public static string PrefabPath = "Prefab2/Component/Icon/EmptyItemIconComponent";

	public static PrefabAuto<EmptyItemIconComponent> PrefabAuto = new PrefabAuto<EmptyItemIconComponent>(PrefabPath);

	public void Open()
	{
	}
}
