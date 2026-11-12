using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NWindow.NPet;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NPet;

public class PetHouseManager : IPetHouseManager, PetAvatar.IListener, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver
{
	public class PetHouseManagerInfo
	{
		public PetHouseCameraController CameraController { get; set; }

		public MapComponent MapComponent { get; set; }

		public IListener IListener { get; set; }

		public IPetHouseProvider IPetHouseProvider { get; set; }
	}

	public class ChangedPetInfo
	{
		public int PetIndex;

		public EPetActionType EPetActionType;
	}

	public interface IListener
	{
		void TouchedPet(PetAvatar petAvatar);
	}

	private static readonly UserObserverManager<ChangedPetInfo> _observerManager = new UserObserverManager<ChangedPetInfo>();

	private readonly float TouchedTime = 0.01f;

	private GameObject _petRootGameObj;

	private PetHouseManagerInfo _info;

	private List<PetAvatar> _petList;

	private DateTime? _pressedTime;

	private Coroutine _coPlacePet;

	private ChangedPetInfo _changedPetInfo = new ChangedPetInfo();

	private bool _isInteracting;

	public static IUserObserverManager ObserverManager => _observerManager;

	private bool CheckIsOpenWindow => SceneAutoBehaviour<PetManagement>.instance.GetInstance<PetList>().IsShow;

	private float RandomXRange
	{
		get
		{
			if (_info == null || _info.IPetHouseProvider == null)
			{
				return 0f;
			}
			return UnityEngine.Random.Range(_info.IPetHouseProvider.MoveXRange.Min, _info.IPetHouseProvider.MoveXRange.Max);
		}
	}

	private float RandomZRange
	{
		get
		{
			if (_info == null || _info.IPetHouseProvider == null)
			{
				return 0f;
			}
			return UnityEngine.Random.Range(_info.IPetHouseProvider.MoveZRange.Min, _info.IPetHouseProvider.MoveZRange.Max);
		}
	}

	private float RandomRangeIdleWaitSeconds => UnityEngine.Random.Range(3f, 12f);

	private bool IsShowPetHouseUI
	{
		get
		{
			if (_info == null || _info.IPetHouseProvider == null)
			{
				return false;
			}
			return _info.IPetHouseProvider.IsShowPetHouseUI;
		}
	}

	public PetHouseManager(PetHouseManagerInfo info)
	{
		_info = info;
		CreatePetRoot();
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
	}

	private IEnumerator CoPlacePet(float sec, List<PlacePetInfo> placePetInfoList)
	{
		while (CheckIsOpenWindow)
		{
			yield return null;
		}
		ClosePetList();
		yield return new WaitForSeconds(sec);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(placePetInfoList) || placePetInfoList.Count <= 0)
		{
			yield break;
		}
		for (int i = 0; i < placePetInfoList.Count; i++)
		{
			PlacePetInfo placePetInfo = placePetInfoList[i];
			if (placePetInfo == null)
			{
				continue;
			}
			IPetAction petAction = AddPet(placePetInfo.PetIndex, i);
			if (petAction == null)
			{
				continue;
			}
			if (CheckIsSleep(GetPetInfo(placePetInfo.PetIndex)))
			{
				placePetInfo.IsPlace = false;
			}
			if (placePetInfo.IsPlace)
			{
				if (i == 0)
				{
					PetSlide(petAction);
				}
				else
				{
					PetHappy(petAction);
				}
			}
		}
	}

	private void Close()
	{
		ClosePetList();
		CloseCoroutine();
		_info = null;
		VespaObject.DestroyGo(_petRootGameObj);
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
	}

	private void CloseCoroutine()
	{
		if (_coPlacePet != null)
		{
			VespaCoroutine.Stop(_coPlacePet);
			_coPlacePet = null;
		}
	}

	private void ClosePetList()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_petList) && _petList.Count > 0)
		{
			IPet pet = null;
			for (int i = 0; i < _petList.Count; i++)
			{
				((IPet)_petList[i])?.Close();
			}
		}
		if (_petList != null)
		{
			_petList.Clear();
			_petList = null;
		}
	}

	private void InitPetList()
	{
		_petList = new List<PetAvatar>();
		_petList.Clear();
	}

	private void CreatePetRoot()
	{
		if (_info != null && !(_info.CameraController == null) && !(_info.CameraController.Cinema == null) && !(_info.CameraController.Cinema.ContentsRoot == null))
		{
			_petRootGameObj = new GameObject();
			_petRootGameObj.transform.name = "PetRoot";
			_petRootGameObj.transform.SetParent(_info.CameraController.Cinema.ContentsRoot.transform);
		}
	}

	private PetAvatar AddPet(int petIndex, int placeIndex)
	{
		if (petIndex <= 0)
		{
			return null;
		}
		if (_petList == null)
		{
			InitPetList();
		}
		PetAvatar petAvatar = CreatePet(petIndex, placeIndex);
		if (petAvatar == null)
		{
			return null;
		}
		_petList.Add(petAvatar);
		return petAvatar;
	}

	private PetAvatar CreatePet(int petIndex, int placeIndex)
	{
		if (petIndex <= 0)
		{
			return null;
		}
		if (_info == null || _info.CameraController == null || _info.CameraController.Cinema == null)
		{
			return null;
		}
		PetInfo petInfo = GetPetInfo(petIndex);
		if (petInfo == null)
		{
			return null;
		}
		PetAvatar petAvatar = new PetAvatar();
		Mediator mediator = new Mediator();
		mediator.Avatar = petAvatar;
		mediator.AvatarController = new AvatarController(mediator);
		NGame2.NUI.NWorldMap.NMapLayout.Avatar.AvatarInfo avatarInfo = new NGame2.NUI.NWorldMap.NMapLayout.Avatar.AvatarInfo
		{
			AvatarController = mediator.AvatarController
		};
		float placedRandomXRange = GetPlacedRandomXRange(placeIndex);
		float placedRandomZRange = GetPlacedRandomZRange(placeIndex);
		PetAvatar.PetAvatarInfo petAvatarInfo = new PetAvatar.PetAvatarInfo
		{
			Cinema = _info.CameraController.Cinema,
			PetInfo = petInfo,
			AvatarInfo = avatarInfo,
			AddCheckHIt = false,
			IListener = this,
			Position = new Vector3(placedRandomXRange, 0f, placedRandomZRange)
		};
		petAvatar.Open(petAvatarInfo, _info.MapComponent);
		if (_petRootGameObj != null && petAvatar.RenderFacade != null && petInfo.CreatureData != null)
		{
			petAvatar.RenderFacade.name = petInfo.CreatureData.ModelCode;
			petAvatar.RenderFacade.transform.SetParent(_petRootGameObj.transform);
		}
		return petAvatar;
	}

	private PetAvatar FindPet(int petIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petList))
		{
			return null;
		}
		IPet pet = null;
		for (int i = 0; i < _petList.Count; i++)
		{
			pet = _petList[i];
			if (pet != null && pet.PetIndex == petIndex)
			{
				return _petList[i];
			}
		}
		return null;
	}

	private PetInfo GetPetInfo(int petIndex)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petIndex);
	}

	private void PetSlide(IPetAction iPetAction)
	{
		if (_info != null && _info.IPetHouseProvider != null)
		{
			iPetAction?.Slide(_info.IPetHouseProvider.SlidePetStartPosition);
		}
	}

	private void PetHappy(IPetAction iPetAction)
	{
		if (_info != null && _info.IPetHouseProvider != null)
		{
			iPetAction?.Happy();
		}
	}

	private float GetPlacedRandomXRange(int placeIndex)
	{
		if (_info == null || _info.IPetHouseProvider == null)
		{
			return 0f;
		}
		float max = _info.IPetHouseProvider.MoveXRange.Max;
		float min = _info.IPetHouseProvider.MoveXRange.Min;
		float num = (max + min) / 2f;
		float num2 = num / 5f;
		switch (placeIndex)
		{
		case 1:
		case 2:
			return UnityEngine.Random.Range(min, num - num2);
		case 3:
		case 4:
			return UnityEngine.Random.Range(num + num2, max);
		default:
			return num;
		}
	}

	private float GetPlacedRandomZRange(int placeIndex)
	{
		if (_info == null || _info.IPetHouseProvider == null)
		{
			return 0f;
		}
		float max = _info.IPetHouseProvider.MoveZRange.Max;
		float min = _info.IPetHouseProvider.MoveZRange.Min;
		float num = (max + min) / 2f;
		float num2 = num / 5f;
		switch (placeIndex)
		{
		case 1:
		case 3:
			return UnityEngine.Random.Range(min, num - num2);
		case 2:
		case 4:
			return UnityEngine.Random.Range(num + num2, max);
		default:
			return num;
		}
	}

	private void RandomPetAction(IPetAction iPetAction)
	{
		if (iPetAction != null && iPetAction.EPetActionType == EPetActionType.None)
		{
			switch ((EPetActionType)Enum.ToObject(typeof(EPetActionType), UnityEngine.Random.Range(2, 5)))
			{
			case EPetActionType.Idle:
				iPetAction.Idle(RandomRangeIdleWaitSeconds);
				break;
			case EPetActionType.Walk:
				iPetAction.Walk(new Vector3(RandomXRange, 0f, RandomZRange));
				break;
			case EPetActionType.Run:
				iPetAction.Run(new Vector3(RandomXRange, 0f, RandomZRange));
				break;
			}
		}
	}

	private void AllPetEnable(bool isEnable, IPet exceptioniPet = null)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petList))
		{
			return;
		}
		IPet pet = null;
		for (int i = 0; i < _petList.Count; i++)
		{
			pet = _petList[i];
			if (pet != null && (exceptioniPet == null || !exceptioniPet.Equals(pet)))
			{
				pet.Enable(isEnable);
			}
		}
	}

	private void SetInitCamera(bool enable)
	{
		if (_info != null && !(_info.CameraController == null))
		{
			_info.CameraController.SetInitCamera(enable);
		}
	}

	private void StartCameraFocus(PetAvatar petAvatar)
	{
		if (petAvatar != null && !(petAvatar.RenderFacade == null) && _info != null && !(_info.CameraController == null))
		{
			Vector3 position = petAvatar.RenderFacade.transform.position;
			position.y += 0.5f;
			_info.CameraController.StartFocusPet(position);
		}
	}

	private bool CheckIsSleep(PetInfo petInfo)
	{
		return PetHelper.CheckIsSleep(petInfo);
	}

	private void AnnounceChangedPetInfo(int petIndex, EPetActionType ePetActionType)
	{
		if (_observerManager != null)
		{
			if (_changedPetInfo == null)
			{
				_changedPetInfo = new ChangedPetInfo();
			}
			_changedPetInfo.PetIndex = petIndex;
			_changedPetInfo.EPetActionType = ePetActionType;
			_observerManager.AnnounceToObservers(_changedPetInfo);
		}
	}

	private bool CheckHitPetAvatar(PetAvatar petAvatar, RaycastHit hit)
	{
		if (petAvatar == null)
		{
			return false;
		}
		if (!_pressedTime.HasValue)
		{
			return false;
		}
		if ((DateTime.UtcNow - _pressedTime.Value).TotalSeconds < (double)TouchedTime)
		{
			return false;
		}
		if (petAvatar.TouchInteractionComponent == null)
		{
			return false;
		}
		return petAvatar.TouchInteractionComponent.CheckRaycastHit(hit);
	}

	private void UpdatePetList(UICamera.MouseOrTouch touch)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petList) || _info == null || _info.CameraController == null || _info.CameraController.Camera == null)
		{
			return;
		}
		Vector2 vector = Vector2.zero;
		bool flag = false;
		if (touch != null)
		{
			vector = touch.pos;
			flag = touch.dragStarted;
			if (!_pressedTime.HasValue)
			{
				_pressedTime = DateTime.UtcNow;
			}
		}
		else
		{
			_pressedTime = null;
		}
		Physics.Raycast(Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(_info.CameraController.Camera, vector), out var hitInfo, 500f);
		IPet pet = null;
		IPetAction petAction = null;
		bool flag2 = false;
		for (int i = 0; i < _petList.Count; i++)
		{
			pet = _petList[i];
			if (pet == null)
			{
				continue;
			}
			flag2 = !flag && CheckHitPetAvatar(_petList[i], hitInfo) && IsShowPetHouseUI;
			pet.Update(flag2, _info.CameraController.NotTouchCamera, CheckIsSleep(pet.PetInfo));
			UpdatePetInteractionState(_petList[i]);
			if (!_isInteracting)
			{
				petAction = _petList[i];
				if (petAction != null && petAction.EPetActionType == EPetActionType.None)
				{
					RandomPetAction(petAction);
				}
			}
		}
	}

	private void UpdatePetInteractionState(IPetInteraction iPetInteraction)
	{
		if (iPetInteraction != null)
		{
			if (_isInteracting)
			{
				iPetInteraction.DisableInteractionState();
			}
			else
			{
				iPetInteraction.Update();
			}
		}
	}

	void IPetHouseManager.Update(UICamera.MouseOrTouch touch)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_petList) && _info != null && !(_info.CameraController == null) && _info.CameraController.ECameraState_ != PetHouseCameraController.ECameraState.FocusPetHouseObject)
		{
			UpdatePetList(touch);
		}
	}

	void IPetHouseManager.Close()
	{
		Close();
	}

	void IPetHouseManager.PlacePet(float sec, List<PlacePetInfo> placePetInfoList)
	{
		CloseCoroutine();
		_coPlacePet = VespaCoroutine.Start(CoPlacePet(sec, placePetInfoList));
	}

	void IPetHouseManager.AllPetEnable(bool isEnable)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_petList))
		{
			IPet pet = null;
			for (int i = 0; i < _petList.Count; i++)
			{
				((IPet)_petList[i])?.Enable(isEnable);
			}
		}
	}

	void IPetHouseManager.NextPetInteraction(int currentPeIndex, int nextIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petList))
		{
			return;
		}
		IPet pet = null;
		IPetAction petAction = null;
		IPetInteraction petInteraction = null;
		int num = 0;
		for (int i = 0; i < _petList.Count; i++)
		{
			pet = _petList[i];
			petAction = _petList[i];
			if (petAction != null && pet.PetIndex == currentPeIndex && (petAction.EPetActionType == EPetActionType.Interaction || petAction.EPetActionType == EPetActionType.Sleep))
			{
				petInteraction = _petList[i];
				num = i;
				break;
			}
		}
		petInteraction?.End(isNext: true);
		int num2 = num + nextIndex;
		if (num2 >= _petList.Count)
		{
			num2 = 0;
		}
		else if (num2 < 0)
		{
			num2 = _petList.Count - 1;
		}
		((IPetInteraction)_petList[num2])?.Start(0f);
	}

	void IPetHouseManager.AnnouncePetsCurrentState()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petList))
		{
			return;
		}
		IPet pet = null;
		IPetAction petAction = null;
		for (int i = 0; i < _petList.Count; i++)
		{
			pet = _petList[i];
			petAction = _petList[i];
			if (pet != null && petAction != null)
			{
				AnnounceChangedPetInfo(pet.PetIndex, petAction.EPetActionType);
			}
		}
	}

	void PetAvatar.IListener.StartInteration(PetAvatar petAvatar)
	{
		if (petAvatar != null && _info != null && _info.IListener != null)
		{
			_isInteracting = true;
			StartCameraFocus(petAvatar);
			AllPetEnable(isEnable: false, petAvatar);
			_info.IListener.TouchedPet(petAvatar);
			_info.CameraController.ECameraState_ = PetHouseCameraController.ECameraState.FocusPet;
		}
	}

	void PetAvatar.IListener.EndInteration(bool isNext)
	{
		SetInitCamera(enable: true);
		AllPetEnable(isEnable: true);
		if (!isNext)
		{
			_isInteracting = false;
		}
	}

	void PetAvatar.IListener.Changed(int petIndex, EPetActionType ePetActionType)
	{
		AnnounceChangedPetInfo(petIndex, ePetActionType);
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedInfo)
	{
		if (changedInfo != null)
		{
			PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(changedInfo.PetIndex);
			if (petInfo != null)
			{
				((IPet)FindPet(changedInfo.PetIndex))?.SetPetInfo(petInfo);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}
}
