using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventStepReward;

public class NodeGenerator
{
	private int _areaWidth;

	private int _areaHeight;

	private int _colLimit;

	private Func<Transform, EventStepRewardComponent> _componentCreateFunc;

	public void SetComponentCreateFunc(Func<Transform, EventStepRewardComponent> componentCreateFunc)
	{
		_componentCreateFunc = componentCreateFunc;
	}

	private EventStepRewardComponent CreateComponent(Transform nodeRoot)
	{
		return Vespa_System_Action_Extension_Method.fire(_componentCreateFunc, nodeRoot);
	}

	public NodeGenerator(int AreaWidth, int AreaHeight, int ColLimit)
	{
		_areaWidth = AreaWidth;
		_areaHeight = AreaHeight;
		_colLimit = ColLimit;
	}

	public List<EventStepRewardComponent> Generate(EventStepRewardComponent.Listener listener, EventStepRewardView.ViewDatas _viewata, Transform nodeRoot)
	{
		List<NPCFriendlyPointData> pointDataList = _viewata.PointDataList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(pointDataList))
		{
			Debug.Log("rewardList is empty!!!");
			return null;
		}
		List<EventStepRewardComponent> list = new List<EventStepRewardComponent>();
		pointDataList.Sort((NPCFriendlyPointData a, NPCFriendlyPointData b) => a.Step.CompareTo(b.Step));
		int num = pointDataList.Max((NPCFriendlyPointData nPCFriendlyPointData) => nPCFriendlyPointData.Step);
		int num2 = num - 1;
		int areaWidth = _areaWidth;
		int areaHeight = _areaHeight;
		int num3 = Mathf.CeilToInt((float)(num2 - 1) / (float)_colLimit);
		int num4 = 0;
		int num5 = 0;
		for (int num6 = 0; num6 < num2; num6++)
		{
			EventStepRewardComponent eventStepRewardComponent = CreateComponent(nodeRoot);
			if (!(eventStepRewardComponent == null))
			{
				EventStepRewardComponent.ENodeType eNodeType = EventStepRewardComponent.ENodeType.Start;
				EventStepRewardComponent.ERewardType eRewardType = EventStepRewardComponent.ERewardType.NONE;
				EventStepRewardComponent.ECustomDir eCustomDir = EventStepRewardComponent.ECustomDir.Right;
				EventStepRewardComponent.ECustomDir eCustomDir2 = EventStepRewardComponent.ECustomDir.Top;
				float num7 = (float)num4 / (float)_colLimit;
				float t = (float)num5 / (float)(num3 - 1);
				float x = ((num5 % 2 == 0) ? Mathf.Lerp(0f, areaWidth, num7) : Mathf.Lerp(0f, areaWidth, 1f - num7)) - (float)(areaWidth / 2);
				float y = 0f - Mathf.Lerp(0f, areaHeight, t) + (float)(areaHeight / 2);
				eNodeType = ((num6 != 0) ? ((num6 != num2 - 1) ? EventStepRewardComponent.ENodeType.Bridge : EventStepRewardComponent.ENodeType.End) : EventStepRewardComponent.ENodeType.Start);
				int num8 = 0;
				if (num4 == _colLimit)
				{
					eCustomDir = EventStepRewardComponent.ECustomDir.Bot;
					num8 = (int)((float)areaHeight / (float)(num3 - 1));
				}
				else
				{
					num8 = areaWidth / _colLimit;
					eCustomDir = ((num5 % 2 != 0) ? EventStepRewardComponent.ECustomDir.Left : EventStepRewardComponent.ECustomDir.Right);
				}
				eCustomDir2 = ((num5 == 0) ? ((num3 > 1 && num4 == _colLimit) ? EventStepRewardComponent.ECustomDir.Bot : EventStepRewardComponent.ECustomDir.Top) : ((num5 == num3 - 1) ? EventStepRewardComponent.ECustomDir.Bot : ((num4 >= _colLimit) ? EventStepRewardComponent.ECustomDir.Bot : EventStepRewardComponent.ECustomDir.Top)));
				if (num6 >= num2 - 1)
				{
					eRewardType = ((_viewata.CurrStep < num2) ? EventStepRewardComponent.ERewardType.LastReward : ((pointDataList.Last() == null || pointDataList.Last().EndRewardRepeatCount == 0) ? EventStepRewardComponent.ERewardType.Repeatable : ((_viewata.CurrRewardStep < num) ? EventStepRewardComponent.ERewardType.LastReward : EventStepRewardComponent.ERewardType.LimitedRepeat)));
					eCustomDir2 = ((num5 % 2 != 0) ? EventStepRewardComponent.ECustomDir.Left : EventStepRewardComponent.ECustomDir.Right);
				}
				else
				{
					eRewardType = EventStepRewardComponent.ERewardType.StepReward;
				}
				list.Add(eventStepRewardComponent);
				switch (eRewardType)
				{
				case EventStepRewardComponent.ERewardType.Repeatable:
					eventStepRewardComponent.Open(pointDataList.Last(), listener, num6, eNodeType, eCustomDir, eCustomDir2, eRewardType, num8, new Vector2(x, y));
					break;
				case EventStepRewardComponent.ERewardType.LimitedRepeat:
					eventStepRewardComponent.Open(pointDataList.Last(), listener, num6, eNodeType, eCustomDir, eCustomDir2, eRewardType, num8, new Vector2(x, y));
					break;
				default:
					eventStepRewardComponent.Open(pointDataList[num6], listener, num6, eNodeType, eCustomDir, eCustomDir2, eRewardType, num8, new Vector2(x, y));
					break;
				}
				num4++;
				if (num4 > _colLimit)
				{
					num4 = 0;
					num5++;
				}
			}
		}
		return list;
	}
}
