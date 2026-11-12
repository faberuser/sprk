using System;
using System.Collections.Generic;
using System.Linq;
using NShared;
using NShared.NWorldMap;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public abstract class IndicatorManager<TEventSubData> : IEventManagerComponent where TEventSubData : class, ITimerEvent
{
	protected EventManager _eventManager;

	protected EventType _eventType;

	protected bool _useEatEffect;

	protected List<NodeIndicator<TEventSubData>> _indicatorList = new List<NodeIndicator<TEventSubData>>();

	public abstract NodeIndicator<TEventSubData> CreateIndicator(WorldMapEventInfo worldMapEventInfo);

	public virtual void Open(EventManager eventManager)
	{
		_eventManager = eventManager;
	}

	public virtual void Close()
	{
		ClearIndicatorImmediately(eatEffect: false);
	}

	public virtual void ChainUpdate()
	{
		UpdateIndicator();
	}

	public virtual void Visit()
	{
	}

	void IEventManagerComponent.Setup(WorldMapEventInfo worldMapEventInfo)
	{
		NodeIndicator<TEventSubData> nodeIndicator = CreateIndicator(worldMapEventInfo);
		if (nodeIndicator != null)
		{
			_indicatorList.Add(nodeIndicator);
		}
	}

	void IEventManagerComponent.Remove(WorldMapEventInfo worldMapEventInfo, bool eatEffect)
	{
		NodeIndicator<TEventSubData> indicator = GetIndicator(worldMapEventInfo);
		if (indicator != null)
		{
			RemoveIndicator(indicator, eatEffect);
		}
	}

	public virtual void OnNodeChanged(int dungeonIndex)
	{
	}

	void IEventManagerComponent.OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd)
	{
	}

	private void UpdateIndicator()
	{
		for (int i = 0; i < _indicatorList.Count; i++)
		{
			NodeIndicator<TEventSubData> nodeIndicator = _indicatorList[i];
			if (nodeIndicator == null)
			{
				_indicatorList.RemoveAt(i);
				i--;
				continue;
			}
			TEventSubData data = nodeIndicator.Data;
			if (data == null || data.IsExpired())
			{
				nodeIndicator.Close(eatEffect: false);
				_indicatorList.RemoveAt(i);
				i--;
			}
		}
	}

	protected void ClearIndicatorImmediately(bool eatEffect)
	{
		for (int i = 0; i < _indicatorList.Count; i++)
		{
			_indicatorList[i].CloseImmediately();
		}
		_indicatorList.Clear();
	}

	protected NodeIndicator<TEventSubData> FindIndicatorByData(Func<TEventSubData, bool> selector)
	{
		for (int i = 0; i < _indicatorList.Count; i++)
		{
			NodeIndicator<TEventSubData> nodeIndicator = _indicatorList[i];
			TEventSubData data = nodeIndicator.Data;
			if (data != null && Vespa_System_Action_Extension_Method.fire(selector, data))
			{
				return nodeIndicator;
			}
		}
		return null;
	}

	protected NodeIndicator<TEventSubData> FindIndicator(Func<NodeIndicator<TEventSubData>, bool> selector)
	{
		for (int i = 0; i < _indicatorList.Count; i++)
		{
			NodeIndicator<TEventSubData> nodeIndicator = _indicatorList[i];
			if (Vespa_System_Action_Extension_Method.fire(selector, nodeIndicator))
			{
				return nodeIndicator;
			}
		}
		return null;
	}

	protected NodeIndicator<TEventSubData> GetIndicator(WorldMapEventInfo worldMapEventInfo)
	{
		if (worldMapEventInfo == null)
		{
			return null;
		}
		if (_eventType != EventType.None && worldMapEventInfo.EventType != _eventType)
		{
			return null;
		}
		int chapterIndex = worldMapEventInfo.ChapterIndex;
		int dungeonIndex = worldMapEventInfo.DungeonIndex;
		return _indicatorList.FirstOrDefault((NodeIndicator<TEventSubData> e) => e.ChapterIndex == chapterIndex && e.DungeonIndex == dungeonIndex);
	}

	protected bool RemoveIndicator(NodeIndicator<TEventSubData> indicator, bool eatEffect)
	{
		for (int i = 0; i < _indicatorList.Count; i++)
		{
			if (_indicatorList[i] == indicator)
			{
				_indicatorList.RemoveAt(i);
				indicator.Close(eatEffect);
				return true;
			}
		}
		return false;
	}
}
