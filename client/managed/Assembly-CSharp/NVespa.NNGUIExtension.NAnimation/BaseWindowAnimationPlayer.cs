using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class BaseWindowAnimationPlayer
{
	public class SequenceAnimation
	{
		public bool IgnoreTimeScale;

		public Action StartAction;

		public List<IUpdater> ListUpdater;

		public Action EndAction;

		public bool IsVaid()
		{
			return !__Extension_Method_ICollection__.IsNullOrEmpty(ListUpdater);
		}

		public SequenceAnimation(BaseAnimationPlayData animationPlayData, Action startAction, Action endAction)
		{
			CreateUpdaterStock stock = CreateUpdaterStock(animationPlayData.IsReverse);
			IgnoreTimeScale = animationPlayData.IgnoreTimeScale;
			ListUpdater = animationPlayData.GetUpdaterList(null, stock);
			StartAction = startAction;
			EndAction = endAction;
		}

		private CreateUpdaterStock CreateUpdaterStock(bool isReverse)
		{
			return new CreateUpdaterStock
			{
				IsReverse = isReverse,
				Delay = 0f
			};
		}
	}

	public BaseWindow BaseWindow;

	public UIPanel ClippingPanel;

	private Queue<SequenceAnimation> _queueingSequenceAnimation = new Queue<SequenceAnimation>();

	private List<IUpdater> _listUpdater;

	private List<IUpdater> _listRemovedUpdater = new List<IUpdater>();

	private Action _endAction;

	private bool _ignoreTimeScale;

	private float _value;

	public void Setup(BasePanel hidePanel, BasePanel showPanel)
	{
		if (hidePanel == null && showPanel == null)
		{
			return;
		}
		ResetAll();
		if (hidePanel != null)
		{
			UIComponetAnimationPlayData showHideAnimationPlayData = hidePanel.ShowHideAnimationPlayData;
			if (showHideAnimationPlayData != null && showHideAnimationPlayData.IsValid())
			{
				showHideAnimationPlayData.IsReverse = true;
				SequenceAnimation item = new SequenceAnimation(showHideAnimationPlayData, null, hidePanel.Hide);
				_queueingSequenceAnimation.Enqueue(item);
			}
			else
			{
				hidePanel.Hide();
			}
		}
		if (showPanel != null)
		{
			UIComponetAnimationPlayData showHideAnimationPlayData2 = showPanel.ShowHideAnimationPlayData;
			if (showHideAnimationPlayData2 != null && showHideAnimationPlayData2.IsValid())
			{
				showHideAnimationPlayData2.IsReverse = false;
				SequenceAnimation item2 = new SequenceAnimation(showHideAnimationPlayData2, showPanel.Show, null);
				_queueingSequenceAnimation.Enqueue(item2);
			}
			else
			{
				showPanel.Show();
			}
		}
		SetNextAniamtion();
	}

	public bool IsEnd()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUpdater))
		{
			return true;
		}
		for (int i = 0; i < _listUpdater.Count(); i++)
		{
			if (!_listUpdater[i].IsEnd())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsExistQueueing()
	{
		return _queueingSequenceAnimation.Count > 0;
	}

	public bool CheckUpdate()
	{
		if (!IsEnd())
		{
			return true;
		}
		if (IsExistQueueing() && IsEnd())
		{
			ResetSequenceAnimation();
			SetNextAniamtion();
			return true;
		}
		return false;
	}

	protected virtual float GetUpdateValue(BaseData.EUpdateValueType updateType)
	{
		switch (updateType)
		{
		case BaseData.EUpdateValueType.Time:
			if (!_ignoreTimeScale)
			{
				return Time.deltaTime;
			}
			return RealTime.deltaTime;
		case BaseData.EUpdateValueType.ScrollViewHorizontal:
			if (!(ClippingPanel != null))
			{
				return 0f;
			}
			return ClippingPanel.UIAnimationScrollPosition.x;
		case BaseData.EUpdateValueType.ScrollViewVertical:
			if (!(ClippingPanel != null))
			{
				return 0f;
			}
			return ClippingPanel.UIAnimationScrollPosition.y;
		default:
			return 0f;
		}
	}

	public void SetNextAniamtion()
	{
		if (_queueingSequenceAnimation.Count > 0)
		{
			SequenceAnimation sequenceAnimation = _queueingSequenceAnimation.Dequeue();
			if (sequenceAnimation != null)
			{
				Vespa_System_Action_Extension_Method.fire(sequenceAnimation.StartAction);
				_ignoreTimeScale = sequenceAnimation.IgnoreTimeScale;
				_listUpdater = sequenceAnimation.ListUpdater;
				_endAction = sequenceAnimation.EndAction;
			}
		}
	}

	public void OnUpdate()
	{
		if (!CheckUpdate())
		{
			return;
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_listUpdater) - 1; num >= 0; num--)
		{
			IUpdater updater = _listUpdater[num];
			_value = GetUpdateValue(updater.UpdateValueType);
			if (!updater.IsEnd())
			{
				updater.OnUpdate(_value);
			}
			else
			{
				_listRemovedUpdater.Add(updater);
				_listUpdater.Remove(updater);
			}
		}
		if (IsEnd())
		{
			Vespa_System_Action_Extension_Method.fire(_endAction);
		}
	}

	public void Close()
	{
		ResetAll();
	}

	private void ResetAll()
	{
		_queueingSequenceAnimation.Clear();
		ResetSequenceAnimation();
	}

	private void ResetSequenceAnimation()
	{
		_endAction = null;
		_ignoreTimeScale = false;
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
}
