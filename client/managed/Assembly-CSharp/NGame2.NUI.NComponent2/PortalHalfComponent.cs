using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PortalHalfComponent : PortalContentsComponent
{
	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalHalfComponent";

	public static PrefabAuto<PortalHalfComponent> PrefabAuto = new PrefabAuto<PortalHalfComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalHalfComponent");

	protected override void UpdateResetTime()
	{
	}
}
