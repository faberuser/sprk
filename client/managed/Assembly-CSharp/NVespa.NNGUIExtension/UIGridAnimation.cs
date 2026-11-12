using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa.NNGUIExtension.NAnimation;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIGrid))]
public class UIGridAnimation : MonoBehaviour, IInspectorGuiDrawer
{
	public enum EGridAnimationShowType
	{
		RepositionOnce,
		RepositionAlways
	}

	public enum ESettingType
	{
		OnlyActive,
		All
	}

	private List<WeakReference> _listUIAnimationPlayer = new List<WeakReference>();

	[HideInInspector]
	public UIGridAnimationPlayData PlayData;

	[HideInInspector]
	public UIGridAnimationPlayData BackwardPlayData;

	[HideInInspector]
	public EGridAnimationShowType GridAnimationShowType;

	[HideInInspector]
	public ESettingType SettingType;

	[HideInInspector]
	public List<EventDelegate> OnStartEvents = new List<EventDelegate>();

	[HideInInspector]
	public List<EventDelegate> OnEndEvents = new List<EventDelegate>();

	private bool _isForwardPlay;

	private bool _isBackwardPlay;

	private BaseWindow _baseWindow;

	private bool _isPlaying;

	private void Awake()
	{
		if (_baseWindow == null)
		{
			_baseWindow = GetComponentInParent<BaseWindow>();
		}
		SingletonAuto<LinkedTransform<UIGridAnimation>>.instance.OnAwake(this);
	}

	private void OnEnable()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer))
		{
			_listUIAnimationPlayer.Clear();
		}
		if (_baseWindow != null && _baseWindow.WindowState != BaseWindow.EState.OpenedShow && GridAnimationShowType == EGridAnimationShowType.RepositionOnce)
		{
			_isForwardPlay = false;
			_isBackwardPlay = false;
		}
		_isPlaying = false;
	}

	private void OnDisable()
	{
		if (_baseWindow != null && (_baseWindow.IsClosingWindowAnimating() || !_baseWindow.IsOpen()) && !__Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer))
		{
			_listUIAnimationPlayer.Clear();
		}
		_isPlaying = false;
	}

	private void OnDestroy()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer))
		{
			_listUIAnimationPlayer.Clear();
		}
		SingletonAuto<LinkedTransform<UIGridAnimation>>.instance.OnDestroy(this);
	}

	private void OnStartEvent()
	{
		EventDelegate.Execute(OnStartEvents);
	}

	private void OnEndEvent()
	{
		EventDelegate.Execute(OnEndEvents);
	}

	public void PlayForward(List<Transform> transList, UIGrid.Arrangement arrangement, int maxPerLine)
	{
		if (GridAnimationShowType != EGridAnimationShowType.RepositionOnce || !_isForwardPlay)
		{
			StartAnimation(PlayData, transList, arrangement, maxPerLine);
			_isPlaying = true;
			_isForwardPlay = true;
			OnStartEvent();
		}
	}

	public void PlayBackward(List<Transform> transList, UIGrid.Arrangement arrangement, int maxPerLine)
	{
		if (GridAnimationShowType != EGridAnimationShowType.RepositionOnce || !_isBackwardPlay)
		{
			StartAnimation(BackwardPlayData, transList, arrangement, maxPerLine);
			_isPlaying = true;
			_isBackwardPlay = true;
			OnStartEvent();
		}
	}

	public void StartAnimation(UIGridAnimationPlayData playData, List<Transform> transList, UIGrid.Arrangement arrangement, int maxPerLine)
	{
		if (_baseWindow == null || __Extension_Method_ICollection__.IsNullOrEmpty(transList))
		{
			return;
		}
		int num = 0;
		UIPanel componentInParent = GetComponentInParent<UIPanel>();
		_listUIAnimationPlayer.Clear();
		if (playData.ShowOrderType == EShowOrderType.LeftTop)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(transList); i++)
			{
				Transform transform = transList[i];
				if (!(transform == null) && (SettingType != ESettingType.OnlyActive || transform.gameObject.activeInHierarchy))
				{
					float startTime = GetStartTime(playData, arrangement, maxPerLine, num);
					UIAnimationPlayer uIAnimationPlayer = UIAnimationPlayer.Begin(_baseWindow, playData, transform.gameObject, componentInParent, startTime);
					if (uIAnimationPlayer != null)
					{
						WeakReference item = new WeakReference(uIAnimationPlayer);
						_listUIAnimationPlayer.Add(item);
					}
					num++;
				}
			}
			return;
		}
		for (int num2 = __Extension_Method_ICollection__.COUNT(transList) - 1; num2 >= 0; num2--)
		{
			Transform transform2 = transList[num2];
			if (!(transform2 == null) && (SettingType != ESettingType.OnlyActive || transform2.gameObject.activeInHierarchy))
			{
				float startTime2 = GetStartTime(playData, arrangement, maxPerLine, num);
				UIAnimationPlayer uIAnimationPlayer2 = UIAnimationPlayer.Begin(_baseWindow, playData, transform2.gameObject, componentInParent, startTime2);
				if (uIAnimationPlayer2 != null)
				{
					WeakReference item2 = new WeakReference(uIAnimationPlayer2);
					_listUIAnimationPlayer.Add(item2);
				}
				num++;
			}
		}
	}

	public float GetStartTime(UIGridAnimationPlayData playData, UIGrid.Arrangement arrangement, int maxPerLine, int index)
	{
		int num = 0;
		int num2 = 0;
		switch (arrangement)
		{
		case UIGrid.Arrangement.Horizontal:
			if (maxPerLine > 0)
			{
				num = index % maxPerLine;
				num2 = index / maxPerLine;
			}
			else
			{
				num = index;
				num2 = 0;
			}
			break;
		case UIGrid.Arrangement.Vertical:
			if (maxPerLine > 0)
			{
				num = index / maxPerLine;
				num2 = index % maxPerLine;
			}
			else
			{
				num = 0;
				num2 = index;
			}
			break;
		}
		return (float)num * playData.ColumnDelay + (float)num2 * playData.RowDelay;
	}

	public bool IsEnd()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer))
		{
			return true;
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_listUIAnimationPlayer) - 1; num >= 0; num--)
		{
			WeakReference weakReference = _listUIAnimationPlayer[num];
			if (!weakReference.IsAlive)
			{
				_listUIAnimationPlayer.Remove(weakReference);
			}
			else
			{
				if (!(weakReference.Target as UIAnimationPlayer).IsEnd())
				{
					return false;
				}
				_listUIAnimationPlayer.Remove(weakReference);
			}
		}
		return __Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer);
	}

	protected void Update()
	{
		if (_isPlaying && IsEnd())
		{
			OnEndEvent();
			_isPlaying = false;
		}
	}

	public void Clear()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_listUIAnimationPlayer))
		{
			_listUIAnimationPlayer.Clear();
		}
	}
}
