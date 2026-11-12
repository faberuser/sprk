using System;
using System.Collections.Generic;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGame2.NUI.NWindow.NPet;
using UnityEngine;

namespace NGame2.NUI.NPet;

public class PetHouseCameraController : MonoBehaviour
{
	public class CameraControllerInfo
	{
		public RenderTargetCinema Cinema;

		public IPetHouseProvider IPetHouseProvider;

		public IListener IListener;
	}

	public interface IListener
	{
		void ProceedingInitCamera();

		void EndInitCamera();

		void EndFocus(ECameraState eCameraState);
	}

	public enum ECameraState
	{
		Normal,
		FocusPet,
		FocusPetHouseObject
	}

	private enum EInitCameraState
	{
		Start,
		Proceeding,
		End
	}

	private float ProceedingInitCameraTime = 0.1f;

	public Vector3 CameraDefaultPosition;

	public Vector3 CameraDefaultRotation;

	public float CameraDefaultDistance;

	public float FocusDistance = 3f;

	public float MinZoomDistance = 3f;

	private CameraControllerInfo _info;

	private CinemaCamera.CameraInfoData _cameraInfo;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private Vector2 _prevPos = Vector2.zero;

	private float _prevDistance;

	private float _focusDistance;

	private EInitCameraState _eInitCameraState = EInitCameraState.End;

	private DateTime? _proceedingInitCameraTime;

	private Vector3? _targetPosition;

	private Vector3? _targetDirection;

	public ECameraState ECameraState_ { get; set; }

	public Camera Camera { get; private set; }

	public RenderTargetCinema Cinema { get; private set; }

	public bool IsFocus { get; private set; }

	public UICamera.MouseOrTouch FirstTouch
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_touchList))
			{
				return null;
			}
			if (_touchList.Count == 1)
			{
				return _touchList[0];
			}
			return null;
		}
	}

	private bool IsInitCamera
	{
		get
		{
			if (_eInitCameraState != EInitCameraState.Start)
			{
				return _eInitCameraState == EInitCameraState.Proceeding;
			}
			return true;
		}
	}

	public bool NotTouchCamera => IsInitCamera;

	public void Init(CameraControllerInfo info)
	{
		_info = info;
		if (_info != null && !(_info.Cinema == null))
		{
			Camera = _info.Cinema.MainCamera;
			Cinema = _info.Cinema;
			if (_info.Cinema.CinemaCamera != null)
			{
				_cameraInfo = _info.Cinema.CinemaCamera.CameraInfo;
			}
			_touchList.Clear();
			SetInitCamera(initCamera: true);
			ECameraState_ = ECameraState.Normal;
		}
	}

	public void Close()
	{
		Camera = null;
		_cameraInfo = null;
		_touchList.Clear();
	}

	public void SetInitCamera(bool initCamera)
	{
		_eInitCameraState = ((!initCamera) ? EInitCameraState.End : EInitCameraState.Start);
		_proceedingInitCameraTime = null;
		if (initCamera)
		{
			_proceedingInitCameraTime = DateTime.UtcNow;
		}
	}

	private void ProceedingInitCamera()
	{
		if (_eInitCameraState == EInitCameraState.Start && _proceedingInitCameraTime.HasValue && !((DateTime.UtcNow - _proceedingInitCameraTime.Value).TotalSeconds < (double)ProceedingInitCameraTime))
		{
			_proceedingInitCameraTime = null;
			_eInitCameraState = EInitCameraState.Proceeding;
			if (_info != null && _info.IListener != null)
			{
				_info.IListener.ProceedingInitCamera();
			}
		}
	}

	private void InitCamera()
	{
		if (!IsInitCamera || _cameraInfo == null)
		{
			return;
		}
		ProceedingInitCamera();
		if (Customizing_Extension_Method.Approximately(CameraDefaultDistance, _cameraInfo.TargetDistance))
		{
			_eInitCameraState = EInitCameraState.End;
			if (_info != null && _info.IListener != null)
			{
				_info.IListener.EndInitCamera();
				ECameraState_ = ECameraState.Normal;
			}
		}
		else
		{
			float t = Time.deltaTime * 9f;
			_cameraInfo.TargetPos = Vector3.Lerp(_cameraInfo.TargetPos, CameraDefaultPosition, t);
			_cameraInfo.LookDir = Vector3.Lerp(_cameraInfo.LookDir, CameraDefaultRotation, t);
			_cameraInfo.TargetDistance = Mathf.Lerp(_cameraInfo.TargetDistance, CameraDefaultDistance, t);
		}
	}

	public void StartFocusPet(Vector3 targetPosition)
	{
		SetInitCamera(initCamera: false);
		_focusDistance = FocusDistance;
		_targetPosition = targetPosition;
		_targetDirection = new Vector3(CameraDefaultRotation.x, CameraDefaultRotation.y + 0.2f, CameraDefaultRotation.z);
		IsFocus = true;
	}

	public void StartFocusPetHouseObject(Vector3 targetPosition, float targetDistance, Vector3 targetDirection)
	{
		SetInitCamera(initCamera: false);
		_targetPosition = targetPosition;
		_focusDistance = targetDistance;
		_targetDirection = targetDirection;
		IsFocus = true;
	}

	private void Focus()
	{
		if (_eInitCameraState != EInitCameraState.End)
		{
			EndFocus();
		}
		else
		{
			if (!IsFocus || !_targetPosition.HasValue || !_targetDirection.HasValue || _cameraInfo == null)
			{
				return;
			}
			if (Customizing_Extension_Method.Approximately(_focusDistance, _cameraInfo.TargetDistance))
			{
				EndFocus();
				if (_info != null && _info.IListener != null)
				{
					_info.IListener.EndFocus(ECameraState_);
				}
			}
			else
			{
				float t = Time.deltaTime * 10f;
				_cameraInfo.TargetPos = Vector3.Lerp(_cameraInfo.TargetPos, _targetPosition.Value, t);
				_cameraInfo.LookDir = Vector3.Lerp(_cameraInfo.LookDir, _targetDirection.Value, t);
				_cameraInfo.TargetDistance = Mathf.Lerp(_cameraInfo.TargetDistance, _focusDistance, t);
			}
		}
	}

	private void EndFocus()
	{
		_targetPosition = null;
		_targetDirection = null;
		IsFocus = false;
	}

	private void Update()
	{
		if (IsInitCamera)
		{
			EndFocus();
			InitCamera();
		}
		else
		{
			Focus();
			UpdateCamera();
		}
	}

	private void UpdateCamera()
	{
		if (ECameraState_ != ECameraState.Normal || IsFocus || _cameraInfo == null || _touchList.Count == 0)
		{
			return;
		}
		if (_touchList.Count == 1)
		{
			UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
			if (mouseOrTouch.dragStarted)
			{
				MoveCamera(mouseOrTouch);
				return;
			}
			Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(Camera, mouseOrTouch.pos);
			Debug.DrawRay(ray.origin, ray.direction * 500f, Color.red);
		}
		else
		{
			_ = _touchList.Count;
			_ = 2;
		}
	}

	private void MoveCamera(UICamera.MouseOrTouch touch)
	{
		if (_cameraInfo != null)
		{
			if (_prevPos == Vector2.zero)
			{
				_prevPos = touch.pos;
				return;
			}
			Vector2 normalized = (touch.pos - _prevPos).normalized;
			Vector3 vector = _cameraInfo.TargetPos - new Vector3(0f, normalized.y, normalized.x);
			float num = (CameraDefaultDistance - _cameraInfo.TargetDistance) * CameraDefaultPosition.y * 1f + 0.5f;
			float z = Mathf.Clamp(vector.z, 0f - num, num);
			_cameraInfo.TargetPos = Vector3.Lerp(_cameraInfo.TargetPos, new Vector3(_cameraInfo.TargetPos.x, CameraDefaultPosition.y, z), Time.deltaTime * 8f);
			_prevPos = touch.pos;
		}
	}

	private void ZoomCamera()
	{
		if (_cameraInfo == null)
		{
			return;
		}
		UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
		float num = Vector3.Distance(b: _touchList[1].pos, a: mouseOrTouch.pos);
		if (_prevDistance <= 0f)
		{
			_prevDistance = num;
			return;
		}
		float num2 = num - _prevDistance;
		float num3 = _cameraInfo.TargetDistance;
		if (num2 < 0f)
		{
			num3 -= 0.3f;
		}
		else if (num2 > 0f)
		{
			num3 += 0.3f;
		}
		float b = Mathf.Clamp(num3, MinZoomDistance, CameraDefaultDistance);
		_cameraInfo.TargetDistance = Mathf.Lerp(_cameraInfo.TargetDistance, b, Time.deltaTime * 8f);
		_prevDistance = num;
	}

	public void OnPress()
	{
		OnBeginDrag();
	}

	public void OnRelease()
	{
		OnEndDrag();
		_touchList.Clear();
	}

	public void OnBeginDrag()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
		}
	}

	public void OnEndDrag()
	{
		if (_touchList.Remove(UICamera.currentTouch))
		{
			_prevPos = Vector2.zero;
			_prevDistance = 0f;
		}
	}
}
