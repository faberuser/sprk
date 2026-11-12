using NVespa;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetInteractionComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Prefab2/Component/Pet/PetInteractionComponent";

	public static PrefabAuto<PetInteractionComponent> PrefabAuto = new PrefabAuto<PetInteractionComponent>(PrefabPath);

	public GameObject BalloonRoot;

	public UISprite SpriteFeed;

	private EPetInteractionState _currentEPetInteractionState;

	void InitOnCreate.OnCreate()
	{
		InitInteractionState();
	}

	void InitOnCreate.OnRelease()
	{
		InitInteractionState();
	}

	public void Open(float petHeight)
	{
		Init(petHeight);
	}

	private void Init(float petHeight)
	{
		SetPoition(petHeight);
		SetDirection();
	}

	private void InitInteractionState()
	{
		UIHelper.EnableGameObject(BalloonRoot, enable: false);
		_currentEPetInteractionState = EPetInteractionState.None;
	}

	private void SetPoition(float petHeight)
	{
		base.transform.localPosition = new Vector3(0f, petHeight + 0.3f, 0f);
	}

	private void SetDirection()
	{
		base.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
	}

	public void DisablePetInteractionState()
	{
		if (UIHelper.IsEnabledGameObjet(BalloonRoot))
		{
			InitInteractionState();
		}
	}

	public void UpdatePetInteractionState(EPetInteractionState ePetInteractionState)
	{
		if (_currentEPetInteractionState == ePetInteractionState)
		{
			return;
		}
		if (ePetInteractionState != EPetInteractionState.None)
		{
			SingletonUpdater<Executer>.instance.DelayRun(0.3f, this, delegate
			{
				UIHelper.EnableGameObject(BalloonRoot, enable: true);
			});
			UIHelper.EnableGameObject(SpriteFeed, ePetInteractionState == EPetInteractionState.Feed);
		}
		else
		{
			UIHelper.EnableGameObject(BalloonRoot, enable: false);
		}
		_currentEPetInteractionState = ePetInteractionState;
	}
}
