using System;
using System.Collections.Generic;
using NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;
using NShared;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapViewer : MonoBehaviour, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener, UICamera.IOnPressListener, IObserverManager
{
	public float ZoomSpeed = 1f;

	public UIWidget MapWidget;

	private Mediator _mediator;

	private WorldMapMode _worldMapMode = new WorldMapMode();

	private NodeMenuMode _nodeMenuMode = new NodeMenuMode();

	private List<FocusNodeMode> _focusNodeList = new List<FocusNodeMode>();

	private List<IObserver> _observerList = new List<IObserver>();

	public void Open(Mediator mediator)
	{
		GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI);
		_mediator = mediator;
		_worldMapMode.Open(mediator);
		_nodeMenuMode.Open(mediator);
	}

	public void SetSize(int width, int height)
	{
		if ((bool)MapWidget)
		{
			MapWidget.SetDimensions(width, height);
		}
		OnSetSize();
	}

	public void OnSetSize()
	{
		_worldMapMode.ResizeZoom();
	}

	public float GetNodeMenuWeight()
	{
		return _nodeMenuMode.ModeWeight;
	}

	public IEnumerator<TaskCurrentData> CreateEnterNodeMenuAsync(EntryMenuSettingData menuAppearance, bool skipAnimation)
	{
		bool nodeMenuModeEntered = false;
		_nodeMenuMode.Enter(menuAppearance, skipAnimation, delegate
		{
			nodeMenuModeEntered = true;
		});
		_mediator.Avatar.SetActionBattleMode(isBattle: false);
		_mediator.AvatarController.ForceDirection(NVespa.Math.Rotate(menuAppearance.AvatarDirAngle));
		_mediator.Avatar.SetEntryMenuOffset(isEnter: true);
		while (!nodeMenuModeEntered)
		{
			yield return TaskCurrentData.RUNNING;
		}
		yield return TaskCurrentData.SUCCESS;
	}

	public ITask CreateEnterNodeMenuTask(EntryMenuSettingData menuAppearance, bool skipAnimation)
	{
		SequenceTask sequenceTask = new SequenceTask();
		bool nodeMenuModeEntered = false;
		sequenceTask.AddTask(delegate
		{
			_nodeMenuMode.Enter(menuAppearance, skipAnimation, delegate
			{
				nodeMenuModeEntered = true;
			});
		});
		sequenceTask.AddTask(delegate
		{
			_mediator.Avatar.SetActionBattleMode(isBattle: false);
		});
		sequenceTask.AddTask(delegate
		{
			_mediator.AvatarController.ForceDirection(NVespa.Math.Rotate(menuAppearance.AvatarDirAngle));
		});
		sequenceTask.AddTask(new WaitTask(() => !nodeMenuModeEntered));
		return sequenceTask;
	}

	public IEnumerator<TaskCurrentData> CreateLeaveNodeMenuAsync()
	{
		_mediator.Avatar.SetEntryMenuOffset(isEnter: false);
		bool nodeMenuModeLeft = false;
		_nodeMenuMode.Leave(delegate
		{
			nodeMenuModeLeft = true;
		});
		while (!nodeMenuModeLeft && _mediator.Layout.IsShowMap)
		{
			yield return TaskCurrentData.RUNNING;
		}
		_mediator.Avatar.SetActionBattleMode(isBattle: true);
		yield return TaskCurrentData.SUCCESS;
	}

	public void FocusPositionMode(float orthoSize, Vector2 v2Position, float blendTime, Action onComplete)
	{
		FocusPositionMode focusPositionMode = new FocusPositionMode();
		focusPositionMode.Open(_mediator, orthoSize, v2Position, blendTime, onComplete);
		AddFocusNode(focusPositionMode);
	}

	public void ShowMapHorizontal(float duration, Action onComplete)
	{
		_worldMapMode.SetMaxOrthoSize();
		_ = _mediator.MapWidth;
		float num = (float)_mediator.MapWidth * 0.5f;
		float x = 0f - num;
		float orthoSize = _worldMapMode.OrthoSize;
		ShowMapCustom(orthoSize, new Vector2(x, 0f), 0.1f, new Vector2(num, 0f), duration, onComplete);
	}

	public void ShowMapVertical(float duration, Action onComplete)
	{
		_worldMapMode.SetMaxOrthoSize();
		_ = _mediator.MapHeight;
		float num = (float)_mediator.MapHeight * 0.5f;
		float y = 0f - num;
		float orthoSize = _worldMapMode.OrthoSize;
		ShowMapCustom(orthoSize, new Vector2(0f, y), 0.01f, new Vector2(0f, num), duration, onComplete);
	}

	public void ShowMapCustom(float orthoSize, Vector2 startPos, float startBlendTime, Vector2 endPos, float endBlendTime, Action onComplete)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_focusNodeList))
		{
			ReleaseFocusNode(null);
		}
		if (endBlendTime <= 0f)
		{
			endBlendTime = 1f;
		}
		FocusPositionMode(orthoSize, startPos, startBlendTime, delegate
		{
			FocusPositionMode(orthoSize, endPos, endBlendTime * 0.5f, delegate
			{
				FocusPositionMode(_worldMapMode.OrthoSize, _worldMapMode.ViewPosition, endBlendTime * 0.5f, delegate
				{
					ReleaseFocusNode(null);
					Vespa_System_Action_Extension_Method.fire(onComplete);
				});
			});
		});
	}

	public void FocusNodeMode(float orthoSize, int dungeonIndex, Action onComplete)
	{
		FocusNodeMode focusNodeMode = new FocusNodeMode();
		focusNodeMode.Open(_mediator, orthoSize, dungeonIndex, onComplete);
		AddFocusNode(focusNodeMode);
	}

	public void FocusBuildngMode(float orthoSize, int buildingIndex, Action onComplete)
	{
		FocusNodeMode focusNodeMode = new FocusNodeMode();
		focusNodeMode.OpenByBuilding(_mediator, orthoSize, buildingIndex, onComplete);
		AddFocusNode(focusNodeMode);
	}

	private void AddFocusNode(FocusNodeMode focusNode)
	{
		_focusNodeList.Add(focusNode);
	}

	public void ReleaseFocusNode(Action onComplete)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_focusNodeList))
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		if (_focusNodeList.Count > 1)
		{
			_focusNodeList.RemoveRange(0, _focusNodeList.Count - 1);
		}
		_focusNodeList[_focusNodeList.Count - 1].Leave(onComplete);
		Notify(FocusEvent.Release);
	}

	public void ChainUpdate()
	{
		if (_mediator == null)
		{
			return;
		}
		Camera camera = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI).Camera;
		Camera camera2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Camera;
		Camera camera3 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3DUI).Camera;
		if (camera == null || camera2 == null || camera3 == null)
		{
			return;
		}
		_worldMapMode.ChainUpdate();
		_nodeMenuMode.ChainUpdate();
		for (int i = 0; i < _focusNodeList.Count; i++)
		{
			FocusNodeMode focusNodeMode = _focusNodeList[i];
			focusNodeMode.ChainUpdate();
			if (!focusNodeMode.IsPlaying && !focusNodeMode.IsEnter)
			{
				_focusNodeList.RemoveAt(i);
				i--;
			}
		}
		float orthoSize = _worldMapMode.OrthoSize;
		Vector3 cameraPos = _worldMapMode.ViewPosition;
		Quaternion cameraRot = _worldMapMode.ViewRotation;
		_nodeMenuMode.AdjustCameraInfo(ref orthoSize, ref cameraPos, ref cameraRot);
		for (int j = 0; j < _focusNodeList.Count; j++)
		{
			_focusNodeList[j].AdjustCameraInfo(ref orthoSize, ref cameraPos, ref cameraRot);
		}
		camera.orthographicSize = orthoSize;
		camera2.orthographicSize = orthoSize;
		camera3.orthographicSize = orthoSize;
		camera.transform.localPosition = cameraPos;
		camera2.transform.localPosition = cameraPos;
		camera3.transform.localPosition = cameraPos;
		camera.transform.localRotation = cameraRot;
		camera2.transform.localRotation = cameraRot;
		camera3.transform.localRotation = cameraRot;
	}

	public void ChasePlayer(bool smoothChase = true)
	{
		_worldMapMode.ChasePlayer(smoothChase);
		ReleaseFocusNode(null);
	}

	public void OnDragWorldMap(Vector2 delta)
	{
		_worldMapMode.OnDragWorldMap(delta);
		ReleaseFocusNode(null);
	}

	public void OnDragStartWorldMap()
	{
		_worldMapMode.OnDragStartWorldMap();
		ReleaseFocusNode(null);
	}

	public void OnDragEndWorldMap()
	{
		_worldMapMode.OnDragEndWorldMap();
	}

	public void OnDragStart()
	{
		OnDragStartWorldMap();
	}

	public void OnDragEnd()
	{
		OnDragEndWorldMap();
	}

	public void OnDrag(Vector2 delta)
	{
		OnDragWorldMap(delta);
	}

	public Vector3 ConvertNodePostion(Vector2 position)
	{
		if (_mediator == null)
		{
			return Vector3.zero;
		}
		return _mediator.ConvertNodePosition((int)position.x, (int)position.y);
	}

	public void AddObserver(IObserver observer)
	{
		_observerList.Add(observer);
	}

	public void RemoveObserver(IObserver observer)
	{
		_observerList.Remove(observer);
	}

	public void ClearObserver()
	{
		_observerList.Clear();
	}

	private void Notify(FocusEvent focusEvent)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_observerList); i++)
		{
			_observerList[i].OnEvent(focusEvent);
		}
	}

	void UICamera.IOnPressListener.OnPress(bool isPressed)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_observerList))
		{
			IObserver observer = null;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_observerList); i++)
			{
				_observerList[i]?.OnTouch(isPressed);
			}
		}
	}
}
