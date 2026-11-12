using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PortalSmallComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalSmallComponent";

	public static PrefabAuto<PortalSmallComponent> PrefabAuto = new PrefabAuto<PortalSmallComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalSmallComponent");

	protected override void Init()
	{
		base.Init();
	}

	protected override void UpdateResetTime()
	{
	}
}
