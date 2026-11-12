using NGame2.NGameSetting.NUi;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHelper;

public class GetItemAnimationData
{
	public enum EState
	{
		Ready,
		Delay,
		Playing
	}

	public enum EPlayingState
	{
		None,
		Show3d,
		CanNext
	}

	private UIWidget _rootWidget;

	private UIGame3DWidget _effectWidget;

	private NGame2.NGameSetting.NUi.GetItemAnimationData _animationData;

	private int _numStar;

	private string[] _item3dModelNodeTree;

	private float _delay;

	private float _elapsedTime;

	private float _elapsedTimeCanNext;

	public EState State { get; private set; }

	public EPlayingState PlayingState { get; private set; }

	public ItemData ItemData { get; private set; }

	public bool CanNext
	{
		get
		{
			if (State == EState.Playing)
			{
				return PlayingState == EPlayingState.CanNext;
			}
			return false;
		}
	}

	public void Open(ItemData itemData, UIWidget root, UIGame3DWidget effect, NGame2.NGameSetting.NUi.GetItemAnimationData animationData, int numStar = 0)
	{
		ItemData = itemData;
		_rootWidget = root;
		_effectWidget = effect;
		_animationData = animationData;
		_numStar = numStar;
		_rootWidget.alpha = 0f;
		State = EState.Ready;
		PlayingState = EPlayingState.None;
	}

	public void Close()
	{
		if (_effectWidget != null)
		{
			_effectWidget.Stop(immediatelyClear: true);
			_effectWidget = null;
		}
		if (_rootWidget != null)
		{
			_rootWidget.transform.localScale = Vector3.one;
			_rootWidget.alpha = 1f;
			_rootWidget = null;
		}
	}

	public void Delay(float delay)
	{
		_delay += delay;
	}

	public void Play(bool open3dModelPopup)
	{
		if (State == EState.Ready)
		{
			State = EState.Delay;
			PlayingState = EPlayingState.None;
			_item3dModelNodeTree = null;
			if (open3dModelPopup && ItemData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(ItemData.Item3dModelNodeTree))
			{
				_item3dModelNodeTree = ItemData.Item3dModelNodeTree;
			}
		}
	}

	public void ChainUpdate()
	{
		if (_animationData == null || _rootWidget == null)
		{
			State = EState.Playing;
			PlayingState = EPlayingState.CanNext;
		}
		else
		{
			if (State == EState.Ready)
			{
				return;
			}
			if (State == EState.Delay)
			{
				_delay -= UnitySystemTimer.deltaTime;
				if (!(_delay > 0f))
				{
					State = EState.Playing;
					PlayingState = EPlayingState.None;
					if (_effectWidget != null)
					{
						_effectWidget.Play(null);
					}
				}
			}
			else if (State == EState.Playing)
			{
				UpdatePlayingState();
			}
		}
	}

	private void UpdateIconAnimation()
	{
		_elapsedTimeCanNext += UnitySystemTimer.deltaTime;
		float num = Mathf.InverseLerp(0f, 1f, _elapsedTimeCanNext);
		float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_animationData.AlphaCurve, num, num);
		float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_animationData.ScaleCurve, num, num);
		_rootWidget.alpha = alpha;
		_rootWidget.transform.localScale = Vector3.one * num2;
	}

	private void UpdatePlayingState()
	{
		_elapsedTime += UnitySystemTimer.deltaTime;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_item3dModelNodeTree))
		{
			float num = Mathf.InverseLerp(_animationData.TimeAnimationBefore, _animationData.TimeAnimationTotal, _elapsedTime);
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_animationData.AlphaCurve, num, num);
			float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_animationData.ScaleCurve, num, num);
			_rootWidget.alpha = alpha;
			_rootWidget.transform.localScale = Vector3.one * num2;
		}
		else if (PlayingState == EPlayingState.CanNext)
		{
			UpdateIconAnimation();
		}
		if (PlayingState != EPlayingState.None || !(_elapsedTime >= _animationData.TimeNext))
		{
			return;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_item3dModelNodeTree))
		{
			PlayingState = EPlayingState.CanNext;
			return;
		}
		PlayingState = EPlayingState.Show3d;
		if (GameUIRoot.IsValidInstance && GameUIRoot.instance.GetLayerInfo(_rootWidget.gameObject.layer) != null)
		{
			Item3dModelPopup.PreLoad(ItemData, _item3dModelNodeTree).OpenPopup(delegate
			{
				PlayingState = EPlayingState.CanNext;
			}, _numStar);
		}
		else if (GlobalUIRoot.isValidInstance && GlobalUIRoot.instance.GetLayerInfo(_rootWidget.gameObject.layer) != null)
		{
			Item3dModelPopup.PreLoadGlobal(ItemData, _item3dModelNodeTree).OpenPopup(delegate
			{
				PlayingState = EPlayingState.CanNext;
			}, _numStar);
		}
		else
		{
			PlayingState = EPlayingState.CanNext;
		}
	}
}
