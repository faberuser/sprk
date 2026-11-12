using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NChatting;

public class PortraitFrameComponent : LinkFactoryComponent
{
	public static string PrefabPath = "Prefab2/Component/Chatting/PortraitFrameComponent";

	public static PrefabAuto<PortraitFrameComponent> PrefabAuto = new PrefabAuto<PortraitFrameComponent>(PrefabPath);

	public UIWidget RootWidget;

	private const float STANDARD_PORTRAIT_SIZE = 56f;

	public static PortraitFrameComponent CreateComponent(Transform parent)
	{
		PortraitFrameComponent portraitFrameComponent = PrefabAuto.Factory.Create(parent);
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(portraitFrameComponent.gameObject.transform, parent.transform);
		return portraitFrameComponent;
	}

	public static PortraitFrameComponent CreateComponent(Transform parent, float portraitSize)
	{
		PortraitFrameComponent portraitFrameComponent = PrefabAuto.Factory.Create(parent);
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(portraitFrameComponent.gameObject.transform, parent.transform);
		portraitFrameComponent.SetRootScale(portraitSize / 56f);
		return portraitFrameComponent;
	}

	private void SetRootScale(float scale)
	{
		if (!(RootWidget == null))
		{
			RootWidget.transform.localScale = new Vector3(scale, scale, 1f);
		}
	}
}
