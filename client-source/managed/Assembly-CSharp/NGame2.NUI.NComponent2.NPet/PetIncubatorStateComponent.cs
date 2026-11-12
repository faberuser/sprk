using System;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetIncubatorStateComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickIncubator(int slotIndex);

		void OnClickFinishIncubating(PetIncubatorSlotInfo petIncubatorSlotInfo);

		void OnClickAddSlot(int slotIndex);
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetIncubatorStateComponent";

	public static PrefabAuto<PetIncubatorStateComponent> PrefabAuto = new PrefabAuto<PetIncubatorStateComponent>(PrefabPath);

	[Header("[Incubator Click]")]
	public UIButton Button_Incubator;

	[Header("[Incubator Gauge]")]
	public UIWidget Widget_GaugeRoot;

	public UIClipSprite GaugeFillAmount;

	[Header("[Icon Balloon]")]
	public GameObject GameObject_IconComplete;

	[Header("[Add Incubator]")]
	public UIButton Button_AddIncubator;

	public UISprite Sptire_AddIncubator;

	private PetIncubatorSlotInfo _incubatorSlotInfo;

	private PetIncubatorObjectInfo.IncubatorStateType _currentState;

	private IListener _listener;

	public int SlotIndex { get; private set; }

	public bool CanOpen { get; set; }

	public void Open(int slotIndex, PetIncubatorSlotInfo slotInfo, IListener listener)
	{
		_listener = listener;
		SlotIndex = slotIndex;
		_incubatorSlotInfo = slotInfo;
		Init();
	}

	private void Init()
	{
		UpdateState();
		UpdateComp();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	private void Update()
	{
		if (NeedUpdate())
		{
			UpdateState();
			UpdateComp();
		}
	}

	private bool NeedUpdate()
	{
		if (_currentState != PetIncubatorObjectInfo.IncubatorStateType.Phase1)
		{
			return _currentState == PetIncubatorObjectInfo.IncubatorStateType.Phase2;
		}
		return true;
	}

	public void UpdateState()
	{
		if (_incubatorSlotInfo == null)
		{
			_currentState = PetIncubatorObjectInfo.IncubatorStateType.None;
			return;
		}
		if (_incubatorSlotInfo.SetItemIndex == 0)
		{
			_currentState = PetIncubatorObjectInfo.IncubatorStateType.Phase0;
			return;
		}
		TimeSpan? incubatorRemainTime = PetHelper.GetIncubatorRemainTime(_incubatorSlotInfo);
		if (incubatorRemainTime.HasValue)
		{
			if (incubatorRemainTime.Value.TotalSeconds <= 0.0)
			{
				_currentState = PetIncubatorObjectInfo.IncubatorStateType.Phase3;
			}
			else
			{
				_currentState = PetIncubatorObjectInfo.IncubatorStateType.Phase1;
			}
		}
	}

	private void UpdateComp()
	{
		switch (_currentState)
		{
		case PetIncubatorObjectInfo.IncubatorStateType.None:
			UIHelper.EnableGameObject(Button_AddIncubator, enable: true);
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: false);
			UIHelper.EnableGameObject(GameObject_IconComplete, enable: false);
			UIHelper.SetGrayscaleSprite(Sptire_AddIncubator, !CanOpen);
			break;
		case PetIncubatorObjectInfo.IncubatorStateType.Phase0:
			UIHelper.EnableGameObject(Button_AddIncubator, enable: false);
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: false);
			UIHelper.EnableGameObject(GameObject_IconComplete, enable: false);
			break;
		case PetIncubatorObjectInfo.IncubatorStateType.Phase1:
		case PetIncubatorObjectInfo.IncubatorStateType.Phase2:
		{
			UIHelper.EnableGameObject(Button_AddIncubator, enable: false);
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: true);
			UIHelper.EnableGameObject(GameObject_IconComplete, enable: false);
			float incubatorRemainTimePercent = PetHelper.GetIncubatorRemainTimePercent(_incubatorSlotInfo);
			UIHelper.SetSpriteFillAmount(GaugeFillAmount, incubatorRemainTimePercent);
			break;
		}
		case PetIncubatorObjectInfo.IncubatorStateType.Phase3:
			UIHelper.EnableGameObject(Button_AddIncubator, enable: false);
			UIHelper.EnableGameObject(Widget_GaugeRoot, enable: true);
			UIHelper.EnableGameObject(GameObject_IconComplete, enable: true);
			UIHelper.SetSpriteFillAmount(GaugeFillAmount, 1f);
			break;
		}
		UIHelper.EnableGameObject(Button_Incubator, _currentState != PetIncubatorObjectInfo.IncubatorStateType.None);
	}

	public void DisableComp()
	{
		UIHelper.EnableGameObject(Button_AddIncubator, enable: false);
		UIHelper.EnableGameObject(Button_Incubator, enable: false);
		UIHelper.EnableGameObject(Widget_GaugeRoot, enable: false);
		UIHelper.EnableGameObject(GameObject_IconComplete, enable: false);
	}

	public void RefreshIncubatorState(PetIncubatorSlotInfo slotInfo)
	{
		_incubatorSlotInfo = slotInfo;
		UpdateState();
		UpdateComp();
	}

	public void OnClickBalloonIcon()
	{
		if (_currentState == PetIncubatorObjectInfo.IncubatorStateType.Phase3 && _listener != null)
		{
			_listener.OnClickFinishIncubating(_incubatorSlotInfo);
		}
	}

	public void OnClickAddIncubator()
	{
		if (_listener != null && _currentState == PetIncubatorObjectInfo.IncubatorStateType.None)
		{
			if (!CanOpen)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PETINCUBATOR_SLOT_NOT_AVAILABLE"));
			}
			else
			{
				_listener.OnClickAddSlot(SlotIndex);
			}
		}
	}

	public void OnClickIncubator()
	{
		if (_listener != null && _currentState != PetIncubatorObjectInfo.IncubatorStateType.None && !CanOpen)
		{
			_listener.OnClickIncubator(SlotIndex);
		}
	}
}
