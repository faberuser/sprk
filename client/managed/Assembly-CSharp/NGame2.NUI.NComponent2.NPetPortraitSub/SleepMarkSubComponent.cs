using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class SleepMarkSubComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/SleepMarkSubComponent";

	public static PrefabAuto<SleepMarkSubComponent> PrefabAuto = new PrefabAuto<SleepMarkSubComponent>(PrefabPath);

	public UIWidget MarkWidget;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.SleepMark));
	}
}
