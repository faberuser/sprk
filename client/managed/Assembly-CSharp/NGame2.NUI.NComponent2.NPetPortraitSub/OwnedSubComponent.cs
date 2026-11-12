using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class OwnedSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/OwnedSubComponent";

	public static PrefabAuto<OwnedSubComponent> PrefabAuto = new PrefabAuto<OwnedSubComponent>(PrefabPath);

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.Owned));
	}
}
