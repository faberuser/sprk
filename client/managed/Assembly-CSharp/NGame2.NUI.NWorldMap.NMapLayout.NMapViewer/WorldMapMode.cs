using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NMapViewer;

public class WorldMapMode
{
	private class ChaseMode
	{
		public enum EType
		{
			ChasePlayer,
			FreeMove
		}

		private float _time;

		public EType Type { get; private set; }

		public float Frame
		{
			get
			{
				float time = GameSetting.data.WorldMap.PlayerChase.Time;
				return Mathf.Clamp01((Time.time - _time) / time);
			}
		}

		public AnimationCurve BlendCurve => GameSetting.data.WorldMap.PlayerChase.Curve;

		public Vector3 PrevPos { get; private set; }

		public ChaseMode()
		{
			Type = EType.ChasePlayer;
			_time = -10000f;
		}

		public void Change(EType type, Vector3 prevPos, bool smoothChase)
		{
			if (type != Type)
			{
				if (smoothChase)
				{
					_time = Time.time;
				}
				else
				{
					_time = -10000f;
				}
				PrevPos = prevPos;
				Type = type;
			}
		}
	}

	private struct DragInfo
	{
		public int TouchId;

		public Vector2 CurrentPos;

		public Vector2 Delta;
	}

	private ChaseMode _chaseMode = new ChaseMode();

	private Mediator _mediator;

	private Vector3 _targetViewPos;

	private Vector3 _validViewPos;

	private bool _dragging;

	private IncreaseDic<int, DragInfo> _dragDic = new IncreaseDic<int, DragInfo>();

	private LocalAccountFloat _orthoSize = new LocalAccountFloat(1f, typeof(WorldMapMode).FullName + "_OrthoSize");

	public float OrthoSize => _orthoSize.Value;

	public Vector3 ViewPosition { get; private set; }

	public Quaternion ViewRotation => Quaternion.identity;

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
	}

	public void ChainUpdate()
	{
		if (_mediator != null)
		{
			UpdateDrag();
			UpdateChaseMode();
			UpdateCamera();
		}
	}

	public void ChasePlayer(bool smoothChase = true)
	{
		_chaseMode.Change(ChaseMode.EType.ChasePlayer, _validViewPos, smoothChase);
	}

	public void OnDragWorldMap(Vector2 delta)
	{
		if (_dragging && _dragDic.ContainsKey(UICamera.currentTouchID))
		{
			DragInfo value = new DragInfo
			{
				TouchId = UICamera.currentTouchID,
				CurrentPos = UICamera.currentTouch.pos,
				Delta = UICamera.currentTouch.delta
			};
			_dragDic.AddAndOverride(value.TouchId, value);
		}
	}

	public void OnDragStartWorldMap()
	{
		_dragging = true;
		DragInfo value = new DragInfo
		{
			TouchId = UICamera.currentTouchID,
			CurrentPos = UICamera.currentTouch.pos,
			Delta = UICamera.currentTouch.delta
		};
		_dragDic.AddAndOverride(value.TouchId, value);
	}

	public void OnDragEndWorldMap()
	{
		_dragging = false;
		_dragDic.Clear();
	}

	private void UpdateDrag()
	{
		float axis = Input.GetAxis("Mouse ScrollWheel");
		axis *= -200f;
		ZoomWorldMap(axis);
		if (_dragging && _dragDic.Count != 0)
		{
			if (_dragDic.Count == 1)
			{
				DragWorldMap(_dragDic.GetValueByIndex(0).Delta);
			}
			else if (_dragDic.Count == 2)
			{
				ZoomWorldMap(((_dragDic.GetValueByIndex(0).Delta + _dragDic.GetValueByIndex(1).Delta) / 2f).x);
			}
			for (int i = 0; i < _dragDic.Count; i++)
			{
				DragInfo valueByIndex = _dragDic.GetValueByIndex(i);
				valueByIndex.Delta = Vector2.zero;
				_dragDic.AddAndOverride(valueByIndex.TouchId, valueByIndex);
			}
		}
	}

	private float CalcZoomInValue(Vector2 delta, Vector2 position, Vector2 pivot)
	{
		if (delta.magnitude < Mathf.Epsilon)
		{
			return 0f;
		}
		if (Vector2.Dot(pivot - position, delta) < 0f)
		{
			return 0f - delta.magnitude;
		}
		return delta.magnitude;
	}

	private void ZoomWorldMap(float zoomInValue)
	{
		zoomInValue = SprkFreeCamera.ScaleLobbyZoom(zoomInValue);
		float p = zoomInValue / (float)Screen.height * _mediator.Viewer.ZoomSpeed;
		int activeHeight = GameUIRoot.instance.UIRoot.activeHeight;
		int num = activeHeight * Screen.width / Screen.height;
		float a = (float)_mediator.MapHeight / (float)activeHeight;
		float b = (float)_mediator.MapWidth / (float)num;
		float b2 = Mathf.Min(a, b);
		float b3 = Mathf.Min(_orthoSize.Value * Mathf.Pow(2f, p), b2);
		b3 = Mathf.Max(0.5f, b3);
		_orthoSize.Value = b3;
	}

	private void DragWorldMap(Vector2 delta)
	{
		delta = SprkFreeCamera.ScaleLobbyDrag(delta);
		float a = 1920f / (float)Screen.width;
		float b = 1080f / (float)Screen.height;
		float num = Mathf.Max(a, b);
		_chaseMode.Change(ChaseMode.EType.FreeMove, _validViewPos, smoothChase: false);
		_targetViewPos = _validViewPos;
		_targetViewPos -= new Vector3(delta.x, delta.y, 0f) * num * _orthoSize.Value;
	}

	private void UpdateChaseMode()
	{
		if (_chaseMode.Type != ChaseMode.EType.FreeMove && _mediator.Avatar != null)
		{
			_targetViewPos = _mediator.Avatar.GetLocalPosition();
		}
	}

	private void UpdateCamera()
	{
		Rect cameraArea = GetCameraArea();
		if (_chaseMode.Type == ChaseMode.EType.FreeMove)
		{
			_validViewPos.x = Mathf.Clamp(_targetViewPos.x, cameraArea.xMin, cameraArea.xMax);
			_validViewPos.y = Mathf.Clamp(_targetViewPos.y, cameraArea.yMin, cameraArea.yMax);
		}
		else
		{
			Vector3 b = default(Vector3);
			b.x = Mathf.Clamp(_targetViewPos.x, cameraArea.xMin, cameraArea.xMax);
			b.y = Mathf.Clamp(_targetViewPos.y, cameraArea.yMin, cameraArea.yMax);
			b.z = _targetViewPos.z;
			float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_chaseMode.BlendCurve, _chaseMode.Frame, _chaseMode.Frame);
			_validViewPos = Vector3.Lerp(_chaseMode.PrevPos, b, t);
		}
		ViewPosition = _validViewPos;
	}

	private Rect GetCameraArea()
	{
		float num = (float)GameUIRoot.instance.UIRoot.activeHeight * _orthoSize.Value;
		float num2 = num * (float)Screen.width / (float)Screen.height;
		float num3 = _mediator.MapWidth;
		float num4 = _mediator.MapHeight;
		float num5 = Mathf.Max(0f, num3 - num2);
		float num6 = Mathf.Max(0f, num4 - num);
		return new Rect((0f - num5) * 0.5f, (0f - num6) * 0.5f, num5, num6);
	}

	public float GetMaxOrthoSize()
	{
		int activeHeight = GameUIRoot.instance.UIRoot.activeHeight;
		int num = activeHeight * Screen.width / Screen.height;
		float a = (float)_mediator.MapHeight / (float)activeHeight;
		float b = (float)_mediator.MapWidth / (float)num;
		return Mathf.Min(a, b);
	}

	public void ResizeZoom()
	{
		float maxOrthoSize = GetMaxOrthoSize();
		float value = Mathf.Max(0.5f, maxOrthoSize);
		if (maxOrthoSize < _orthoSize.Value)
		{
			_orthoSize.Value = value;
		}
	}

	public void SetMaxOrthoSize()
	{
		float maxOrthoSize = GetMaxOrthoSize();
		if (maxOrthoSize != _orthoSize.Value)
		{
			_orthoSize.Value = maxOrthoSize;
		}
	}
}
