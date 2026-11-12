using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPetPortraitSub;

public class DisableStatusComponent : BaseSubComponent<PetPortraitComponent>
{
	public static string PrefabPath = "Prefab2/Component/PetPortraitSub/DisableStatusComponent";

	public static PrefabAuto<DisableStatusComponent> PrefabAuto = new PrefabAuto<DisableStatusComponent>(PrefabPath);

	public GameObject ObjRoot;

	public override void Open(PetPortraitComponent owner)
	{
		base.Open(owner);
		SetRootAnchor(base.Owner.GetRootGO(PetPortraitComponent.ESubComponent.DisableStatus));
	}
}
