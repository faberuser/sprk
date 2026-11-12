using System;
using System.Collections.Generic;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NBattle;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NNGUIExtension.NAnimation;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class WindowAnimation : MonoBehaviour, IInspectorGuiDrawer, LateUpdaterAfterPanel.IReceiver
{
	public interface IAlphaAnimator
	{
		void SetAlpha(float alpha);
	}

	public Transform TargetTm;

	public float Duration;

	public bool ManualStart;

	public bool DestroySelf;

	public float LifeTime;

	public ShowType AlphaShowType;

	public AnimationCurve AlphaCurve;

	public ShowType ScaleShowType;

	public AnimationCurve ScaleCurve;

	public ShowType RotationShowType;

	public float RotationAngle;

	public AnimationCurve RotationCurve;

	public ShowType PositionShowType;

	public Vector2 PositionStart;

	public AnimationCurve PositionCurve;

	[HideInInspector]
	public bool UseReversePositionCurve;

	[HideInInspector]
	public AnimationCurve ReversePositionCurve;

	private Transform _root;

	private float _time;

	private bool _started;

	private bool _ended;

	private bool _paused;

	private bool _reverse;

	private int _firstSkipFrame;

	private int _lastSkipFrame;

	private Action _onPlayed;

	private UIPanel[] _panels;

	private Vector3 _initialLocalPosition;

	private Quaternion _initialLocalRotation;

	private Vector3 _initialLocalScale;

	[HideInInspector]
	public bool UseWindowAnimationUpdater;

	[HideInInspector]
	public WindowAnimationPlayData WindowAnimationPlayData;

	private float _reverseDelay;

	private float _animationMaxTime;

	private bool _endedWindowAnim;

	private bool _endedWindowUpdater;

	private bool _updaterTimeOver;

	private List<NVespa.NNGUIExtension.NAnimation.IUpdater> _listUpdater;

	private List<NVespa.NNGUIExtension.NAnimation.IUpdater> _listRemovedUpdater = new List<NVespa.NNGUIExtension.NAnimation.IUpdater>();

	private float _durationSafe => Mathf.Max(Duration, Mathf.Epsilon);

	public void PlayAnimation(bool reverse, Action onPlayed = null)
	{
		_time = 0f;
		_started = true;
		_ended = false;
		_paused = false;
		_reverse = reverse;
		_reverseDelay = 0f;
		_firstSkipFrame = 3;
		_lastSkipFrame = 3;
		_onPlayed = onPlayed;
		_endedWindowAnim = false;
		_endedWindowUpdater = true;
		_updaterTimeOver = true;
		InitData();
		InitUpdaters();
		Init();
		if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing && !SceneAutoBehaviour<EndCampaign>.isValidInstance)
		{
			Complete();
		}
		else if (!HasAnimation())
		{
			Complete();
		}
	}

	public void SetRoot(Transform root)
	{
		_root = root;
		_initialLocalPosition = _root.localPosition;
		_initialLocalRotation = _root.localRotation;
		_initialLocalScale = _root.transform.localScale;
	}

	private void Awake()
	{
		SingletonAuto<LinkedTransform<WindowAnimation>>.instance.OnAwake(this);
		Transform root = TargetTm;
		if (TargetTm == null)
		{
			BaseWindow component = GetComponent<BaseWindow>();
			root = ((!component || !component.WindowRoot) ? base.transform : component.WindowRoot);
		}
		SetRoot(root);
		Vespa_Unity_Component_Extension_Method.GetComponentAuto<LateUpdaterAfterPanel>(this);
	}

	private void OnDestroy()
	{
		SingletonAuto<LinkedTransform<WindowAnimation>>.instance.OnDestroy(this);
	}

	private void OnEnable()
	{
		if (!ManualStart)
		{
			Init();
		}
	}

	private void Init()
	{
		AdjustAll(0f);
	}

	public void Pause()
	{
		_paused = true;
	}

	public void Resume()
	{
		_paused = false;
	}

	private bool IsPlayable(ShowType showType)
	{
		if (showType != ShowType.Both && (showType != ShowType.Start || _reverse))
		{
			if (showType == ShowType.End)
			{
				return _reverse;
			}
			return false;
		}
		return true;
	}

	public bool HasAnimation()
	{
		if (Duration <= 0f)
		{
			return false;
		}
		if (IsPlayable(AlphaShowType))
		{
			return true;
		}
		if (IsPlayable(ScaleShowType))
		{
			return true;
		}
		if (IsPlayable(RotationShowType))
		{
			return true;
		}
		if (IsPlayable(PositionShowType))
		{
			return true;
		}
		if (UseWindowAnimationUpdater && WindowAnimationPlayData.IsValid())
		{
			return true;
		}
		return false;
	}

	public bool IsAnimating(bool reverse)
	{
		if (!_started)
		{
			return false;
		}
		if (_ended)
		{
			return false;
		}
		return _reverse == reverse;
	}

	public void Complete()
	{
		if (_started && !_ended)
		{
			_time = _durationSafe;
			UpdateAnimation(0f);
		}
	}

	public void CompleteWithoutCallback()
	{
		_onPlayed = null;
		_ended = true;
		Complete();
	}

	private void LateUpdate()
	{
	}

	void LateUpdaterAfterPanel.IReceiver.OnUpdate()
	{
		float unscaledDeltaTime = UnitySystemTimer.unscaledDeltaTime;
		UpdateAnimation(unscaledDeltaTime);
	}

	private void UpdateAnimation(float deltaTime)
	{
		if (!_started || _paused)
		{
			return;
		}
		if (_firstSkipFrame > 0)
		{
			_firstSkipFrame--;
			return;
		}
		_time += deltaTime;
		if (DestroySelf && _time >= LifeTime)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			if (_ended)
			{
				return;
			}
			AdjustAll(_time);
			AdjustWindowUpdaters(deltaTime);
			_endedWindowAnim = Duration + _reverseDelay < _time;
			_endedWindowUpdater = IsUpdaterEnd();
			_updaterTimeOver = _animationMaxTime < _time;
			if (!_endedWindowAnim || (!_updaterTimeOver && !_endedWindowUpdater))
			{
				return;
			}
			if (_lastSkipFrame > 0)
			{
				_lastSkipFrame--;
				return;
			}
			_ended = true;
			if (!SystemStatus.IsPlayingCritical)
			{
				AdjustAlpha(1f);
				_root.localPosition = _initialLocalPosition;
				_root.localRotation = _initialLocalRotation;
				_root.localScale = _initialLocalScale;
			}
			Vespa_System_Action_Extension_Method.fire(_onPlayed);
			_onPlayed = null;
		}
	}

	private float GetAnimationTime(float time)
	{
		if (!_reverse)
		{
			return time;
		}
		if (_reverseDelay <= 0f)
		{
			return time;
		}
		if (time <= _reverseDelay)
		{
			return 0f;
		}
		return time - _reverseDelay;
	}

	private float GetFinalRatio(float time)
	{
		if (time == 0f)
		{
			if (!_reverse)
			{
				return 0f;
			}
			return 1f;
		}
		float num = 0f;
		num = time / _durationSafe;
		return Mathf.Clamp01(_reverse ? (1f - num) : num);
	}

	public void AdjustAll(float time)
	{
		float animationTime = GetAnimationTime(time);
		float finalRatio = GetFinalRatio(animationTime);
		AdjustAlpha(finalRatio);
		AdjustScale(finalRatio);
		AdjustRotation(finalRatio);
		AdjustPosition(finalRatio);
	}

	public void AdjustWindowUpdaters(float deltaTime)
	{
		if (IsUpdaterEnd())
		{
			return;
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_listUpdater) - 1; num >= 0; num--)
		{
			NVespa.NNGUIExtension.NAnimation.IUpdater updater = _listUpdater[num];
			if (!updater.IsEnd())
			{
				updater.OnUpdate(deltaTime);
			}
			else
			{
				_listRemovedUpdater.Add(updater);
				_listUpdater.Remove(updater);
			}
		}
	}

	private void AdjustAlpha(float ratio)
	{
		if (!IsPlayable(AlphaShowType))
		{
			return;
		}
		float alpha = Mathf.Clamp01(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, ratio, ratio));
		IAlphaAnimator component = base.gameObject.GetComponent<IAlphaAnimator>();
		if (component != null)
		{
			component.SetAlpha(alpha);
			return;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_panels))
		{
			_panels = GetComponentsInChildren<UIPanel>();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_panels))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_panels); i++)
			{
				_panels[i].alpha = alpha;
			}
		}
	}

	private void AdjustScale(float ratio)
	{
		if (IsPlayable(ScaleShowType))
		{
			float a = Mathf.Max(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(ScaleCurve, ratio, ratio), 0f);
			a = Mathf.Max(a, 0.1f);
			_root.localScale = _initialLocalScale * a;
		}
	}

	private void AdjustRotation(float ratio)
	{
		if (IsPlayable(RotationShowType))
		{
			float num = Mathf.Max(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(RotationCurve, ratio, ratio), 0f);
			Quaternion quaternion = Quaternion.AngleAxis(RotationAngle * num, Vector3.forward);
			_root.localRotation = _initialLocalRotation * quaternion;
		}
	}

	private void AdjustPosition(float ratio)
	{
		if (IsPlayable(PositionShowType))
		{
			float t = ((!UseReversePositionCurve || !_reverse) ? Mathf.Clamp01(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(PositionCurve, ratio, ratio)) : Mathf.Clamp01(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(ReversePositionCurve, ratio, ratio)));
			Vector3 localPosition = _initialLocalPosition + Vector3.Lerp(PositionStart, Vector3.zero, t);
			_root.localPosition = localPosition;
		}
	}

	private bool IsExistUpdater()
	{
		if (!UseWindowAnimationUpdater)
		{
			return false;
		}
		if (!WindowAnimationPlayData.IsValid())
		{
			return false;
		}
		return true;
	}

	private void InitData()
	{
		if (IsExistUpdater())
		{
			_endedWindowUpdater = false;
			_updaterTimeOver = false;
			WindowAnimationPlayData.IsReverse = _reverse;
		}
	}

	public void InitUpdaters()
	{
		if (!IsExistUpdater())
		{
			return;
		}
		ResetUpdaters();
		CreateUpdaterStock stock = new CreateUpdaterStock
		{
			IsReverse = WindowAnimationPlayData.IsReverse
		};
		float num = 0f;
		_listUpdater = WindowAnimationPlayData.GetUpdaterList(new GameObject[1] { base.gameObject }, stock);
		_animationMaxTime = 0f;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(WindowAnimationPlayData.ListDataGroup); i++)
		{
			WindowAnimationPlayData.DataGroup dataGroup = WindowAnimationPlayData.ListDataGroup[i];
			if (dataGroup != null)
			{
				if (dataGroup.AlphaData.IsValid())
				{
					num = ((!_reverse) ? (dataGroup.AlphaData.Delay + dataGroup.AlphaData.Value) : dataGroup.AlphaData.Value);
					_animationMaxTime = Mathf.Max(_animationMaxTime, num);
				}
				if (dataGroup.PositionData.IsValid())
				{
					num = ((!_reverse) ? (dataGroup.PositionData.Delay + dataGroup.PositionData.Value) : dataGroup.PositionData.Value);
					_animationMaxTime = Mathf.Max(_animationMaxTime, num);
				}
				if (dataGroup.RotationData.IsValid())
				{
					num = ((!_reverse) ? (dataGroup.RotationData.Delay + dataGroup.RotationData.Value) : dataGroup.RotationData.Value);
					_animationMaxTime = Mathf.Max(_animationMaxTime, num);
				}
				if (dataGroup.ScaleData.IsValid())
				{
					num = ((!_reverse) ? (dataGroup.ScaleData.Delay + dataGroup.ScaleData.Value) : dataGroup.ScaleData.Value);
					_animationMaxTime = Mathf.Max(_animationMaxTime, num);
				}
			}
		}
		if (_reverse && _animationMaxTime > _durationSafe)
		{
			_reverseDelay = _animationMaxTime - _durationSafe;
		}
		else
		{
			_reverseDelay = 0f;
		}
	}

	public void ResetUpdaters()
	{
		_animationMaxTime = 0f;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listRemovedUpdater); i++)
		{
			_listRemovedUpdater[i]?.Reset();
		}
		_listRemovedUpdater.Clear();
		if (_listUpdater != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listUpdater); j++)
			{
				_listUpdater[j]?.Reset();
			}
			_listUpdater.Clear();
		}
	}

	public bool IsUpdaterEnd()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUpdater))
		{
			return true;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listUpdater); i++)
		{
			if (!_listUpdater[i].IsEnd())
			{
				return false;
			}
		}
		return true;
	}

	public void SetUpdaterEnd()
	{
		for (int num = __Extension_Method_ICollection__.COUNT(_listUpdater) - 1; num >= 0; num--)
		{
			NVespa.NNGUIExtension.NAnimation.IUpdater updater = _listUpdater[num];
			updater.SetEnd();
			_listRemovedUpdater.Add(updater);
			_listUpdater.Remove(updater);
		}
		ResetUpdaters();
	}
}
