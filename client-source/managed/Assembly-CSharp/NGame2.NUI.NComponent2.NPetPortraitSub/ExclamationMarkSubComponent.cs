using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class ExclamationMarkSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/ExclamationMarkSubComponent";

	public static PrefabAuto<ExclamationMarkSubComponent> PrefabAuto = new PrefabAuto<ExclamationMarkSubComponent>(PrefabPath);

	public UIWidget MarkWidget;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.ExclamationMark));
	}
}
