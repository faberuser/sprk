using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PetExploreResultComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Pet/PetExploreResultComponent";

	public static PrefabAuto<PetExploreResultComponent> PrefabAuto = new PrefabAuto<PetExploreResultComponent>("Prefab2/Component/Pet/PetExploreResultComponent");

	public Transform TransformPetRoot;

	public UILabel LabelFood;

	private PetPortraitComponent _petPortraitComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleasePetPortraitComponent();
	}

	public void Open(PetInfo petInfo, int reqFood)
	{
		SetPetPortraitComponent(petInfo);
		SetFoodLabel(petInfo, reqFood);
	}

	private void SetFoodLabel(PetInfo petInfo, int reqFood)
	{
		bool flag = petInfo != null;
		UIHelper.EnableGameObject(LabelFood, flag);
		if (flag)
		{
			int fullPoint = petInfo.FullPoint;
			string stringDirect = LocalizationManager.GetStringDirect("FOOD_DOWN", fullPoint, reqFood);
			UIHelper.SetLabelText(LabelFood, stringDirect);
		}
	}

	private void SetPetPortraitComponent(PetInfo petInfo)
	{
		if (petInfo == null || TransformPetRoot == null)
		{
			ReleasePetPortraitComponent();
			return;
		}
		if (_petPortraitComponent == null)
		{
			_petPortraitComponent = PetPortraitComponent.SmallPrefabAuto.Factory.Create(TransformPetRoot);
		}
		if (_petPortraitComponent != null)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_petPortraitComponent.transform);
			_petPortraitComponent.Open(petInfo, null);
		}
	}

	private void ReleasePetPortraitComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _petPortraitComponent);
	}
}
