using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingHeroContentComponent : MonoBehaviour, InitOnCreate, ILoadingComponent
{
	public const string PrefabPath = "Prefab2/Component/Loading/LoadingHeroContentComponent";

	public static PrefabAuto<LoadingHeroContentComponent> PrefabAuto = new PrefabAuto<LoadingHeroContentComponent>("Prefab2/Component/Loading/LoadingHeroContentComponent");

	public void Open(LoadingData loadingData)
	{
	}

	public bool IsCompleted()
	{
		return true;
	}

	public UIWidget GetAnchorWidget(EAnchorPivot pivot)
	{
		return null;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
