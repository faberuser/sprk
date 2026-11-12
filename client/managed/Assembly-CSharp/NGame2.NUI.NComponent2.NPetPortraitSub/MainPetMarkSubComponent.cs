using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class MainPetMarkSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/MainPetMarkSubComponent";

	public static PrefabAuto<MainPetMarkSubComponent> PrefabAuto = new PrefabAuto<MainPetMarkSubComponent>(PrefabPath);

	public UIWidget MarkWidget;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.MainPet));
	}
}
