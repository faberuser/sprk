using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class NewMarkSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/NewMarkSubComponent";

	public static PrefabAuto<NewMarkSubComponent> PrefabAuto = new PrefabAuto<NewMarkSubComponent>(PrefabPath);

	public UIWidget MarkWidget;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.NewMark));
	}
}
