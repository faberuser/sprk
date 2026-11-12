using System.Collections;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public abstract class RollingImagePanel : MonoBehaviour, IInspectorGuiDrawer
{
	public enum SwipteAmimationTypes
	{
		Basic,
		Clean,
		Hybrid
	}

	public UIPanel MainPanel;

	public UITexture CurrTexture;

	public UITexture NextTexture;

	public float ShowTime = 5f;

	public float SwipeAnimationTime = 1f;

	public AnimationCurve Curve;

	public SwipteAmimationTypes SwipeType;

	[Range(0f, 1f)]
	public float SwipeChangeRange = 0.2f;

	private float _width;

	private float _height;

	private Vector3 _rightOut;

	private Vector3 _leftOut;

	private float _uvX;

	private bool _isPlayingSwipeAnimation;

	private bool _isPressed;

	private Vector3 _touchPos;

	private int _touchId;

	private float _dragDeltaX;

	protected bool _enableSwipe = true;

	protected bool _isForwardSwipeDirection = true;

	private Coroutine _coContinueSwipe;

	private Coroutine _coPlay;

	private void OnEnable()
	{
		UpdateDerived();
	}

	public virtual void UpdateDerived()
	{
		if (_width != MainPanel.width || _height != MainPanel.height)
		{
			_width = MainPanel.width;
			_height = MainPanel.height;
			_rightOut = Vector3.right * _width;
			_leftOut = Vector3.left * _width;
			OnChagneSize();
		}
	}

	public void Play()
	{
		_coPlay = VespaCoroutine.Start(this, CoPlay());
	}

	public void Stop()
	{
		VespaCoroutine.Stop(_coPlay);
	}

	protected IEnumerator CoPlay()
	{
		yield return null;
		float elapsed = 0f;
		while (true)
		{
			if (_isPressed)
			{
				yield return null;
				continue;
			}
			if (_coContinueSwipe != null)
			{
				yield return _coContinueSwipe;
				_coContinueSwipe = null;
				elapsed = 0f;
				continue;
			}
			elapsed += Time.deltaTime;
			if (elapsed < ShowTime)
			{
				yield return null;
				continue;
			}
			if (SetAutoNextImage())
			{
				yield return VespaCoroutine.Start(this, CoSwipe());
			}
			SetCurrentImageFromNext();
			OnImageChange();
			elapsed = 0f;
		}
	}

	private IEnumerator CoSwipe(float continuedT = 0f, bool forward = true, bool calcCurve = true)
	{
		_isPlayingSwipeAnimation = true;
		yield return null;
		float startTime = Time.time;
		continuedT = (forward ? continuedT : Mathf.Abs(1f - continuedT));
		while (true)
		{
			float num = (Time.time - startTime) / SwipeAnimationTime + continuedT;
			if (forward)
			{
				Move(SwipeType, num, calcCurve);
			}
			else
			{
				Move(SwipeType, Mathf.Max(1f - num, 0f), calcCurve);
			}
			if (num >= 1f)
			{
				break;
			}
			yield return null;
		}
		_isPlayingSwipeAnimation = false;
	}

	private void ContinueSwipe(float continuedT = 0f, bool forward = true)
	{
		_coContinueSwipe = VespaCoroutine.Start(this, CoContinueSwipe(continuedT, forward));
	}

	private IEnumerator CoContinueSwipe(float continuedT = 0f, bool forward = true)
	{
		yield return VespaCoroutine.Start(this, CoSwipe(continuedT, forward, calcCurve: false));
		if (forward)
		{
			SetDragNextImage();
			OnImageChange();
		}
	}

	private void Move(SwipteAmimationTypes swipeType, float t, bool calcCurve = true)
	{
		if (calcCurve)
		{
			t = Curve.Evaluate(t);
		}
		switch (swipeType)
		{
		case SwipteAmimationTypes.Basic:
			SwipeAnimation_Basic(t);
			break;
		case SwipteAmimationTypes.Clean:
			SwipeAnimation_Clean(t);
			break;
		case SwipteAmimationTypes.Hybrid:
			SwipeAnimation_Hybrid(t);
			break;
		}
	}

	private void SwipeAnimation_Hybrid(float t)
	{
		Vector3 nextStartPos = (_isForwardSwipeDirection ? _rightOut : _leftOut);
		Vector3 prevEndPos = (_isForwardSwipeDirection ? _leftOut : _rightOut);
		SetPosition(nextStartPos, prevEndPos, t);
		float nextUV = (_isForwardSwipeDirection ? 0.5f : (-0.5f));
		float prevUV = (_isForwardSwipeDirection ? (-0.5f) : 0.5f);
		SetUV(prevUV, nextUV, t);
	}

	private void SwipeAnimation_Clean(float t)
	{
		Vector3 nextStartPos = (_isForwardSwipeDirection ? _rightOut : _leftOut);
		Vector3 prevEndPos = (_isForwardSwipeDirection ? _leftOut : _rightOut);
		SetPosition(nextStartPos, prevEndPos, t);
		float nextUV = (_isForwardSwipeDirection ? 1f : (-1f));
		float prevUV = (_isForwardSwipeDirection ? (-1f) : 1f);
		SetUV(prevUV, nextUV, t);
	}

	private void SwipeAnimation_Basic(float t)
	{
		Vector3 nextStartPos = (_isForwardSwipeDirection ? _rightOut : _leftOut);
		Vector3 prevEndPos = (_isForwardSwipeDirection ? _leftOut : _rightOut);
		SetPosition(nextStartPos, prevEndPos, t);
	}

	private void SetPosition(Vector3 nextStartPos, Vector3 prevEndPos, float t)
	{
		NextTexture.transform.localPosition = Vector3.Lerp(nextStartPos, Vector3.zero, t);
		CurrTexture.transform.localPosition = Vector3.Lerp(Vector3.zero, prevEndPos, t);
	}

	private void SetUV(float prevUV, float nextUV, float t)
	{
		SetUV(NextTexture, Mathf.Lerp(nextUV + _uvX, _uvX, t));
		SetUV(CurrTexture, Mathf.Lerp(_uvX, prevUV + _uvX, t));
	}

	private void SetUV(UITexture texture, float uvX)
	{
		Rect uvRect = texture.uvRect;
		uvRect.x = uvX;
		texture.uvRect = uvRect;
	}

	protected virtual bool SetAutoNextImage()
	{
		_isForwardSwipeDirection = true;
		return true;
	}

	protected abstract void SetNextImage();

	protected abstract void SetPrevImage();

	protected virtual void SetCurrentImageFromNext()
	{
		CurrTexture.mainTexture = NextTexture.mainTexture;
		ResetSwipe();
	}

	protected virtual void SetDragNextImage()
	{
		SetCurrentImageFromNext();
	}

	protected virtual void OnImageChange()
	{
	}

	public void ResetSwipe()
	{
		Move(SwipeType, 0f);
	}

	public virtual void OnTouch()
	{
	}

	public void OnPress(bool pressed)
	{
		if (!_enableSwipe || _isPlayingSwipeAnimation || (pressed && _isPressed) || (!pressed && _touchId != UICamera.currentTouchID))
		{
			return;
		}
		Vector3 touchPos = GetTouchPos();
		if (pressed)
		{
			_touchPos = GetTouchPos();
			_touchId = UICamera.currentTouchID;
		}
		else
		{
			float num = Mathf.Abs(touchPos.x - _touchPos.x);
			if (num == 0f)
			{
				ResetSwipe();
			}
			else if (num >= SwipeChangeRange)
			{
				ContinueSwipe(num);
			}
			else
			{
				ContinueSwipe(num, forward: false);
			}
			_touchId = -10;
			_dragDeltaX = 0f;
		}
		_isPressed = pressed;
	}

	public void OnClick()
	{
		OnTouch();
	}

	public void OnDragStart()
	{
	}

	public void OnDragEnd()
	{
	}

	public void OnDrag(Vector2 delta)
	{
		if (_enableSwipe && _touchId == UICamera.currentTouchID)
		{
			float num = GetTouchPos().x - _touchPos.x;
			if (num > 0f && _dragDeltaX <= 0f)
			{
				_isForwardSwipeDirection = false;
				SetPrevImage();
			}
			else if (num < 0f && _dragDeltaX >= 0f)
			{
				_isForwardSwipeDirection = true;
				SetNextImage();
			}
			Move(SwipeType, Mathf.Abs(num), calcCurve: false);
			_dragDeltaX = num;
		}
	}

	public static Vector3 GetTouchPos()
	{
		Vector2 lastTouchPosition = UICamera.lastTouchPosition;
		lastTouchPosition.x = Mathf.Clamp01(lastTouchPosition.x / (float)Screen.width);
		lastTouchPosition.y = Mathf.Clamp01(lastTouchPosition.y / (float)Screen.height);
		return lastTouchPosition;
	}

	protected void OnChagneSize()
	{
		if (CurrTexture != null && CurrTexture.mainTexture != null)
		{
			CurrTexture.width = (int)_width;
			CurrTexture.height = (int)_height;
			NextTexture.width = (int)_width;
			NextTexture.height = (int)_height;
			int width = CurrTexture.mainTexture.width;
			int height = CurrTexture.mainTexture.height;
			float width2 = _width;
			float height2 = _height;
			if ((float)width * height2 > (float)height * width2)
			{
				float num = (float)height * width2 / height2;
				float width3 = num / (float)width;
				float x = ((float)width - num) / 2f / (float)width;
				ChangeDefalutUV(CurrTexture, x, 0f, width3, 1f);
				ChangeDefalutUV(NextTexture, x, 0f, width3, 1f);
			}
			else if ((float)width * height2 < (float)height * width2)
			{
				float num2 = (float)width * height2 / width2;
				float height3 = num2 / (float)height;
				float y = ((float)height - num2) / 2f / (float)height;
				ChangeDefalutUV(CurrTexture, 0f, y, 1f, height3);
				ChangeDefalutUV(NextTexture, 0f, y, 1f, height3);
			}
			else
			{
				ChangeDefalutUV(CurrTexture, 0f, 0f, 1f, 1f);
				ChangeDefalutUV(NextTexture, 0f, 0f, 1f, 1f);
			}
		}
	}

	private void ChangeDefalutUV(UITexture texture, float x, float y, float width, float height)
	{
		_uvX = x;
		Rect uvRect = texture.uvRect;
		uvRect.width = width;
		uvRect.height = height;
		uvRect.x = x;
		uvRect.y = y;
		texture.uvRect = uvRect;
	}
}
