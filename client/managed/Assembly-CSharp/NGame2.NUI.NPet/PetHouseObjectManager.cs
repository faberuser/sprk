using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NNodeTree;
using NGame2.NRender;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPet;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NPet;

public class PetHouseObjectManager : IPetHouseObjectManager, PetHouseObjectComponent.IListener, IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>, IUserObserver
{
	public class PetHouseObjectManagerInfo
	{
		public PetHouseCameraController CameraController { get; set; }

		public IListener IListener { get; set; }

		public IPetHouseObjectProvider IPetHouseObjectProvider { get; set; }
	}

	public interface IListener
	{
		void TouchedPetHouseObject(IPetHouseObject iPetHouseObject, bool isPossibleFocus, Action onEnd);
	}

	private readonly float TouchedTime = 0.02f;

	private readonly float PetFoodStateUpdateDelayTime = 1f;

	private PetHouseObjectManagerInfo _info;

	private List<PetHouseObjectComponent> _petHouseObjectList;

	private DateTime? _pressedTime;

	private DateTime? _lastPetFoodStateUpdatedTime;

	private EPetHouseObjectType _prevEPetFoodStateEPetHouseObjectType;

	public List<PetIncubatorObjectInfo> PetIncubatorObjectList { get; private set; }

	public PetHouseObjectManager(PetHouseObjectManagerInfo info)
	{
		_info = info;
		Init();
	}

	private void Init()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		InitPetHouseObjectList();
	}

	private void InitPetHouseObjectList()
	{
		if (_info == null || _info.CameraController == null || _info.CameraController.Cinema == null)
		{
			return;
		}
		List<RenderFacade> renderfacades = _info.CameraController.Cinema.GetRenderfacades();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(renderfacades))
		{
			return;
		}
		_petHouseObjectList = new List<PetHouseObjectComponent>();
		_petHouseObjectList.Clear();
		RenderFacade renderFacade = null;
		PetHouseObjectComponent petHouseObjectComponent = null;
		for (int i = 0; i < renderfacades.Count; i++)
		{
			renderFacade = renderfacades[i];
			if (!(renderFacade == null) && renderFacade.AssetCreatureAsync != null && renderFacade.AssetCreatureAsync.Stock.Code.Contains(GetPrefixModelCode(GetPetHouseObjectModelCode(renderFacade))))
			{
				petHouseObjectComponent = CreatePetHouseObject(renderFacade);
				if (petHouseObjectComponent != null)
				{
					_petHouseObjectList.Add(petHouseObjectComponent);
				}
			}
		}
		PetIncubatorObjectList = new List<PetIncubatorObjectInfo>();
		PetIncubatorObjectList.Clear();
		foreach (PetIncubatorSlotInfo item2 in GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager)
		{
			PetIncubatorObjectInfo item = CreateIncubatorObject(item2);
			PetIncubatorObjectList.Add(item);
		}
		RefreshPetFoodState();
	}

	private PetIncubatorObjectInfo CreateIncubatorObject(PetIncubatorSlotInfo slotInfo)
	{
		int slotIndex = slotInfo.SlotIndex;
		int mountingIndex = slotInfo.MountingIndex;
		PetIncubatorSlotData data = GameRoot.Instance.PetIncubatorSlotDataContainer.GetData(slotIndex);
		PetIncubatorData data2 = GameRoot.Instance.PetIncubatorDataContainer.GetData(mountingIndex);
		if (data == null || data2 == null)
		{
			VespaDebug.LogError($"IncubatorObjectData is null : slotIndex {slotIndex}, incubatorIndex {mountingIndex}");
			return null;
		}
		RenderFacade renderFacade = CreatePetHouseIncubator(slotIndex, data2.CreatureCode, data.Position, data.Rotation);
		PetIncubatorObjectInfo.IncubatorStateType incubatorState = GetIncubatorState(slotInfo);
		PetIncubatorObjectInfo petIncubatorObjectInfo = new PetIncubatorObjectInfo();
		petIncubatorObjectInfo.SlotIndex = slotIndex;
		petIncubatorObjectInfo.SlotInfo = slotInfo;
		petIncubatorObjectInfo.RenderFacade = renderFacade;
		petIncubatorObjectInfo.PlayIncubatorActionByState(incubatorState);
		return petIncubatorObjectInfo;
	}

	public static PetIncubatorObjectInfo.IncubatorStateType GetIncubatorState(PetIncubatorSlotInfo slotInfo)
	{
		if (slotInfo == null)
		{
			return PetIncubatorObjectInfo.IncubatorStateType.None;
		}
		if (slotInfo.SetItemIndex == 0)
		{
			return PetIncubatorObjectInfo.IncubatorStateType.Phase0;
		}
		TimeSpan? incubatorRemainTime = PetHelper.GetIncubatorRemainTime(slotInfo);
		if (!incubatorRemainTime.HasValue)
		{
			return PetIncubatorObjectInfo.IncubatorStateType.None;
		}
		if (incubatorRemainTime.Value.TotalSeconds <= 0.0)
		{
			return PetIncubatorObjectInfo.IncubatorStateType.Phase3;
		}
		if ((double)PetHelper.GetIncubatorRemainTimePercent(slotInfo) < 0.5)
		{
			return PetIncubatorObjectInfo.IncubatorStateType.Phase1;
		}
		return PetIncubatorObjectInfo.IncubatorStateType.Phase2;
	}

	private void Close()
	{
		if (PetIncubatorObjectList != null)
		{
			foreach (PetIncubatorObjectInfo petIncubatorObject in PetIncubatorObjectList)
			{
				_info.CameraController.Cinema.DestroyCreature(petIncubatorObject.SlotIndex);
			}
			PetIncubatorObjectList.Clear();
			PetIncubatorObjectList = null;
		}
		_info = null;
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_petHouseObjectList) && _petHouseObjectList.Count > 0)
		{
			IPetHouseObject petHouseObject = null;
			for (int i = 0; i < _petHouseObjectList.Count; i++)
			{
				((IPetHouseObject)_petHouseObjectList[i])?.Close();
			}
		}
		if (_petHouseObjectList != null)
		{
			_petHouseObjectList.Clear();
			_petHouseObjectList = null;
		}
		_pressedTime = null;
	}

	private void UpdatePetHouesObjectState()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petHouseObjectList))
		{
			return;
		}
		UpdatePetFoodState();
		IPetHouseObject petHouseObject = null;
		for (int i = 0; i < _petHouseObjectList.Count; i++)
		{
			petHouseObject = _petHouseObjectList[i];
			if (petHouseObject != null && petHouseObject.IsEnable)
			{
				petHouseObject.Update();
			}
		}
	}

	private void UpdateIncubatorState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PetIncubatorObjectList); i++)
		{
			PetIncubatorObjectList[i].UpdateIncubatorState();
		}
	}

	private void AddNewIncubatorObject(int slotIndex)
	{
		PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(slotIndex);
		if (petIncubatorSlotInfo != null)
		{
			PetIncubatorObjectInfo item = CreateIncubatorObject(petIncubatorSlotInfo);
			PetIncubatorObjectList.Add(item);
		}
	}

	private void ChangeIncubatorObject(int slotIndex)
	{
		PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(slotIndex);
		if (petIncubatorSlotInfo == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PetIncubatorObjectList); i++)
		{
			PetIncubatorObjectInfo petIncubatorObjectInfo = PetIncubatorObjectList[i];
			if (slotIndex == petIncubatorObjectInfo.SlotIndex)
			{
				_info.CameraController.Cinema.DestroyCreature(petIncubatorObjectInfo.SlotIndex);
				PetIncubatorObjectList.Remove(petIncubatorObjectInfo);
				break;
			}
		}
		PetIncubatorObjectInfo item = CreateIncubatorObject(petIncubatorSlotInfo);
		PetIncubatorObjectList.Add(item);
	}

	void IPetHouseObjectManager.SetIncubatorState(int slotIndex, PetIncubatorObjectInfo.IncubatorStateType incubatorStateType, Action onEnd, bool needInfoUpdate)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PetIncubatorObjectList); i++)
		{
			if (slotIndex == PetIncubatorObjectList[i].SlotIndex)
			{
				PetIncubatorObjectList[i].PlayIncubatorActionByState(incubatorStateType, onEnd, needInfoUpdate);
			}
		}
	}

	void IPetHouseObjectManager.EnableNameComponentList(bool isEnable)
	{
		EnableNameComponentList(isEnable);
	}

	private void UpdatePetFoodState()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!_lastPetFoodStateUpdatedTime.HasValue)
		{
			_lastPetFoodStateUpdatedTime = utcNow;
		}
		if (!((utcNow - _lastPetFoodStateUpdatedTime.Value).TotalSeconds < (double)PetFoodStateUpdateDelayTime))
		{
			RefreshPetFoodState();
			_lastPetFoodStateUpdatedTime = utcNow;
		}
	}

	public void RefreshPetFoodState()
	{
		long remainPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		long maxPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MaxPetFoodCount;
		EPetHouseObjectType ePetHouseObjectType = EPetHouseObjectType.None;
		ePetHouseObjectType = ((remainPetFoodCount <= 0) ? EPetHouseObjectType.FeedEmpty : ((remainPetFoodCount < maxPetFoodCount) ? EPetHouseObjectType.FeedNormal : EPetHouseObjectType.FeedMax));
		if (_prevEPetFoodStateEPetHouseObjectType == ePetHouseObjectType)
		{
			return;
		}
		IPetHouseObject petHouseObject = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petHouseObjectList); i++)
		{
			petHouseObject = _petHouseObjectList[i];
			if (petHouseObject != null && CheckFeedPetHouseObjectType(petHouseObject.EPetHouseObjectType))
			{
				bool enable = petHouseObject.EPetHouseObjectType == ePetHouseObjectType;
				petHouseObject.Enable(enable);
			}
		}
		_prevEPetFoodStateEPetHouseObjectType = ePetHouseObjectType;
	}

	private RenderFacade CreatePetHouseIncubator(int slotIndex, string code, float[] position, float[] rotation)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(position) || __Extension_Method_ICollection__.COUNT(position) < 3)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rotation) || __Extension_Method_ICollection__.COUNT(rotation) < 3)
		{
			return null;
		}
		if (_info == null || _info.CameraController == null || _info.CameraController.Cinema == null)
		{
			return null;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, code, 5);
		if (renderFacadeBuilder == null)
		{
			return null;
		}
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		RenderTargetCinema cinema = _info.CameraController.Cinema;
		int uid = cinema.SpawnCreature(VariableKeyword.Myself, slotIndex, renderFacadeBuilder);
		RenderFacade rENDERFACADE = cinema.GETRENDERFACADE(uid);
		if (rENDERFACADE == null)
		{
			return null;
		}
		rENDERFACADE.transform.localScale = Vector3.one;
		rENDERFACADE.transform.position = new Vector3(position[0], position[1], position[2]);
		rENDERFACADE.transform.eulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
		rENDERFACADE.Action.SetDirection(VectorXZ.zero, blendMotion: true);
		return rENDERFACADE;
	}

	private PetHouseObjectComponent CreatePetHouseObject(RenderFacade renderFacade)
	{
		if (renderFacade == null)
		{
			return null;
		}
		string petHouseObjectModelCode = GetPetHouseObjectModelCode(renderFacade);
		EPetHouseObjectType ePetHouseObjectType = GetEPetHouseObjectType(petHouseObjectModelCode);
		if (ePetHouseObjectType == EPetHouseObjectType.None)
		{
			return null;
		}
		PetHouseObjectComponent petHouseObjectComponent = new PetHouseObjectComponent();
		PetHouseObjectComponent.PetHouseObjectInfo info = new PetHouseObjectComponent.PetHouseObjectInfo
		{
			RenderFacade = renderFacade,
			EPetHouseObjectType = ePetHouseObjectType,
			NameComponentLocalPositionOffset = GetNameComponentLocalPositionOffset(ePetHouseObjectType),
			NameComponentScale = GetNameComponentScale(ePetHouseObjectType),
			IsPlayTouchAnimation = GetEnableTouchAnimation(ePetHouseObjectType),
			IListener = this,
			LocalizationKey = GetPetHouseObjectLocalizationKey(ePetHouseObjectType)
		};
		petHouseObjectComponent.Open(info);
		return petHouseObjectComponent;
	}

	private Vector3 GetNameComponentLocalPositionOffset(EPetHouseObjectType ePetHouseObjectType)
	{
		if (_info == null || _info.IPetHouseObjectProvider == null)
		{
			return Vector3.zero;
		}
		return _info.IPetHouseObjectProvider.GetPetHouseObjectInfo(ePetHouseObjectType)?.NameComponentLocalPositionOffset ?? Vector3.zero;
	}

	private float GetNameComponentScale(EPetHouseObjectType ePetHouseObjectType)
	{
		if (_info == null || _info.IPetHouseObjectProvider == null)
		{
			return 1f;
		}
		return _info.IPetHouseObjectProvider.GetPetHouseObjectInfo(ePetHouseObjectType)?.NameComponetScale ?? 1f;
	}

	private bool GetEnableTouchAnimation(EPetHouseObjectType ePetHouseObjectType)
	{
		if (_info == null || _info.IPetHouseObjectProvider == null)
		{
			return false;
		}
		return _info.IPetHouseObjectProvider.GetPetHouseObjectInfo(ePetHouseObjectType)?.IsPlayTouchAnimation ?? false;
	}

	private string GetPetHouseObjectLocalizationKey(EPetHouseObjectType ePetHouseObjectType)
	{
		if (_info == null || _info.IPetHouseObjectProvider == null)
		{
			return string.Empty;
		}
		PetHouseWindow.PetHouseObjectInfo petHouseObjectInfo = _info.IPetHouseObjectProvider.GetPetHouseObjectInfo(ePetHouseObjectType);
		if (petHouseObjectInfo == null)
		{
			return string.Empty;
		}
		return petHouseObjectInfo.LocalizationKey;
	}

	private bool CheckFeedPetHouseObjectType(EPetHouseObjectType ePetHouseObjectType)
	{
		if (ePetHouseObjectType == EPetHouseObjectType.FeedNormal || ePetHouseObjectType == EPetHouseObjectType.FeedEmpty || ePetHouseObjectType == EPetHouseObjectType.FeedMax)
		{
			return true;
		}
		return false;
	}

	private EPetHouseObjectType GetEPetHouseObjectType(string modelCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(modelCode))
		{
			return EPetHouseObjectType.None;
		}
		string[] array = modelCode.Split('.');
		if (array.Length <= 2)
		{
			return EPetHouseObjectType.None;
		}
		string value = array[2];
		if (!Enum.IsDefined(typeof(EPetHouseObjectType), value))
		{
			return EPetHouseObjectType.None;
		}
		return (EPetHouseObjectType)Enum.Parse(typeof(EPetHouseObjectType), value);
	}

	private string GetPrefixModelCode(string modelCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(modelCode))
		{
			return string.Empty;
		}
		string[] array = modelCode.Split('.');
		if (array.Length <= 2)
		{
			return string.Empty;
		}
		return array[0] + "." + array[1];
	}

	private string GetPetHouseObjectModelCode(RenderFacade renderFacade)
	{
		if (renderFacade == null)
		{
			return string.Empty;
		}
		if (renderFacade.AssetCreatureAsync == null)
		{
			return string.Empty;
		}
		return renderFacade.AssetCreatureAsync.Stock.Code;
	}

	private bool CheckHitHouseObject(IPetHouseObject iPetHouseObject, RaycastHit hit)
	{
		if (iPetHouseObject == null)
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
		return iPetHouseObject.CheckHit(hit);
	}

	private void EnableNameComponentList(bool isEnable)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petHouseObjectList); i++)
		{
			_petHouseObjectList[i]?.EnableNameComponent(isEnable);
		}
	}

	private bool CanUsePetHouseObject(IPetHouseObject focusIPetHouseObject)
	{
		if (focusIPetHouseObject.EPetHouseObjectType == EPetHouseObjectType.IncubatorHouse || focusIPetHouseObject.EPetHouseObjectType == EPetHouseObjectType.Mirror || focusIPetHouseObject.EPetHouseObjectType == EPetHouseObjectType.Jukebox)
		{
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			return false;
		}
		return true;
	}

	private void FocusPetHouseObject(IPetHouseObject focusIPetHouseObject, Action onEnd = null)
	{
		if (!CanUsePetHouseObject(focusIPetHouseObject))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_USE_FACILITY_CONDITION_TEXT"));
		}
		else
		{
			if (focusIPetHouseObject == null || __Extension_Method_ICollection__.IsNullOrEmpty(_petHouseObjectList) || _info == null || _info.CameraController == null)
			{
				return;
			}
			IPetHouseObject petHouseObject = null;
			for (int i = 0; i < _petHouseObjectList.Count; i++)
			{
				petHouseObject = _petHouseObjectList[i];
				if (petHouseObject != null && petHouseObject.EPetHouseObjectState == EPetHouseObjectState.Focus)
				{
					return;
				}
			}
			if (_info.IListener == null || _info.IPetHouseObjectProvider == null)
			{
				return;
			}
			PetHouseWindow.PetHouseObjectInfo petHouseObjectInfo = _info.IPetHouseObjectProvider.GetPetHouseObjectInfo(focusIPetHouseObject.EPetHouseObjectType);
			if (petHouseObjectInfo != null)
			{
				if (petHouseObjectInfo.IsPossibleFocus)
				{
					_info.CameraController.ECameraState_ = PetHouseCameraController.ECameraState.FocusPetHouseObject;
					focusIPetHouseObject.EPetHouseObjectState = EPetHouseObjectState.Focus;
					Vector3 targetPosition = focusIPetHouseObject.PetHouseObjectPosition + petHouseObjectInfo.TargetPositionOffset;
					float targetDistance = petHouseObjectInfo.TargetDistance;
					Vector3 targetDirection = petHouseObjectInfo.TargetDirection;
					_info.CameraController.StartFocusPetHouseObject(targetPosition, targetDistance, targetDirection);
				}
				EnableNameComponentList(!petHouseObjectInfo.IsPossibleFocus);
				_info.IListener.TouchedPetHouseObject(focusIPetHouseObject, petHouseObjectInfo.IsPossibleFocus, onEnd);
			}
		}
	}

	private IPetHouseObject GetIPetHouseObject(EPetHouseObjectType ePetHouseObjectType)
	{
		IPetHouseObject petHouseObject = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_petHouseObjectList); i++)
		{
			petHouseObject = _petHouseObjectList[i];
			if (petHouseObject != null && petHouseObject.EPetHouseObjectType == ePetHouseObjectType)
			{
				return petHouseObject;
			}
		}
		return null;
	}

	void IPetHouseObjectManager.Update(UICamera.MouseOrTouch touch)
	{
		UpdatePetHouesObjectState();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petHouseObjectList) || _info == null || _info.CameraController == null || _info.CameraController.Camera == null)
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
		if (flag || !_pressedTime.HasValue)
		{
			return;
		}
		Physics.Raycast(Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(_info.CameraController.Camera, vector), out var hitInfo, 500f);
		IPetHouseObject petHouseObject = null;
		for (int i = 0; i < _petHouseObjectList.Count; i++)
		{
			petHouseObject = _petHouseObjectList[i];
			if (petHouseObject != null && petHouseObject.IsEnable && CheckHitHouseObject(petHouseObject, hitInfo))
			{
				petHouseObject.Touch();
				break;
			}
		}
	}

	void IPetHouseObjectManager.AlwaysUpdate()
	{
		UpdateIncubatorState();
	}

	void IPetHouseObjectManager.Close()
	{
		Close();
	}

	void IPetHouseObjectManager.SetStateIdleAllPetHouseObject()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_petHouseObjectList))
		{
			return;
		}
		IPetHouseObject petHouseObject = null;
		for (int i = 0; i < _petHouseObjectList.Count; i++)
		{
			petHouseObject = _petHouseObjectList[i];
			if (petHouseObject != null)
			{
				petHouseObject.EPetHouseObjectState = EPetHouseObjectState.Idle;
			}
		}
	}

	void IPetHouseObjectManager.FocusPetHouseObject(EPetHouseObjectType ePetHouseObjectType, Action onEnd)
	{
		if (ePetHouseObjectType == EPetHouseObjectType.None)
		{
			if (onEnd != null)
			{
				Vespa_System_Action_Extension_Method.fire(onEnd);
			}
		}
		else
		{
			FocusPetHouseObject(GetIPetHouseObject(ePetHouseObjectType), onEnd);
		}
	}

	void IPetHouseObjectManager.AddNewIncubatorObject(int slotIndex)
	{
		AddNewIncubatorObject(slotIndex);
	}

	void IPetHouseObjectManager.ChangeIncubatorObject(int slotIndex)
	{
		ChangeIncubatorObject(slotIndex);
	}

	void PetHouseObjectComponent.IListener.Touch(IPetHouseObject touchIPetHouseObject)
	{
		FocusPetHouseObject(touchIPetHouseObject);
	}

	void IUserObserver<NGame2.NAccount.UserPetManager.ChangedPetInfo>.OnChanged(NGame2.NAccount.UserPetManager.ChangedPetInfo changedPetInfo)
	{
		if (changedPetInfo != null && (changedPetInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.AddedPet || changedPetInfo.EChangedPetInfoType == NGame2.NAccount.UserPetManager.EChangedPetInfoType.RemoveAddedPet))
		{
			GetIPetHouseObject(EPetHouseObjectType.Management)?.RefreshNotice();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
