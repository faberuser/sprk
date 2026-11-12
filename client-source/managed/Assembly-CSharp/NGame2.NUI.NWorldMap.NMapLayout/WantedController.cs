using System;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class WantedController : AvatarController
{
	private CrowdMediator _mediator;

	private LocalStorageInt _lastPathNo;

	private LocalStorageDateTime _lastPathDate;

	public int WantedIndex { get; private set; }

	public WantedQuestData QuestData { get; private set; }

	public WantedPathData PathData { get; private set; }

	public int PathNo { get; private set; }

	public WantedController(IListener listener, CrowdMediator mediator, int wantedIndex)
		: base(listener)
	{
		WantedIndex = wantedIndex;
		_mediator = mediator;
		_lastPathNo = new LocalStorageInt(0, "WantedController_LastPathNo_" + wantedIndex);
		_lastPathDate = new LocalStorageDateTime(NGame2.NUI.NWindow.Common.GetUTCNow(), "WantedController_LastPathDate_" + wantedIndex);
	}

	public void InitPosition()
	{
		QuestData = GameRoot.Instance.WantedQuestDataContainer.GetData(WantedIndex);
		if (QuestData == null)
		{
			VespaDebug.LogError("Wanted Quest not found : " + WantedIndex);
			return;
		}
		SpeedFactor = QuestData.SpeedFactor;
		PathData = GameRoot.Instance.WantedPathDataContainer.GetData(QuestData.PathIndex);
		if (PathData == null)
		{
			VespaDebug.LogError("Wanted Path not found : wanted=" + WantedIndex + " path=" + QuestData.PathIndex);
		}
		else
		{
			if (__Extension_Method_ICollection__.COUNT(PathData.DungeonIndices) == 0)
			{
				return;
			}
			float speed = GetSpeed();
			if (Mathf.Approximately(speed, 0f))
			{
				int num = PathData.DungeonIndices[0];
				_mediator.SetDungeonIndex(num);
				_listener.GetAvatar().SetPositionByDungeonIndex(num);
				return;
			}
			int num2 = __Extension_Method_ICollection__.COUNT(PathData.DungeonIndices);
			float num3 = 0f;
			Vector3 a = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(PathData.DungeonIndices[0]);
			for (int i = 1; i <= num2; i++)
			{
				int dungeonIndex = PathData.DungeonIndices[i % num2];
				Vector3 nodePositionByDungeonIndex = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(dungeonIndex);
				float num4 = Vector3.Distance(a, nodePositionByDungeonIndex);
				num3 += num4;
				a = nodePositionByDungeonIndex;
			}
			if (Mathf.Approximately(num3, 0f))
			{
				return;
			}
			float num5 = num3 / speed;
			int value = _lastPathNo.Value;
			DateTime value2 = _lastPathDate.Value;
			float num6 = (float)(NGame2.NUI.NWindow.Common.GetUTCNow() - value2).TotalSeconds;
			num6 %= num5;
			int num7 = value;
			int dungeonIndex2 = PathData.DungeonIndices[num7];
			Vector3 vector = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(dungeonIndex2);
			Vector3 position = vector;
			float num8 = 0f;
			int num9 = 0;
			while (num9++ < 100)
			{
				int num10 = (num7 + 1) % num2;
				int num11 = PathData.DungeonIndices[num10];
				Vector3 nodePositionByDungeonIndex2 = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(num11);
				float num12 = Vector3.Distance(vector, nodePositionByDungeonIndex2) / speed;
				if (num8 + num12 > num6)
				{
					float num13 = num6 - num8;
					float t = ((num12 > 0f) ? Mathf.Clamp01(num13 / num12) : 0f);
					position = Vector3.Lerp(vector, nodePositionByDungeonIndex2, t);
					break;
				}
				num8 += num12;
				num7 = num10;
				dungeonIndex2 = num11;
				vector = nodePositionByDungeonIndex2;
				position = nodePositionByDungeonIndex2;
			}
			PathNo = num7;
			_mediator.SetDungeonIndex(dungeonIndex2);
			_listener.GetAvatar().SetPosition(position);
			int num14 = (PathNo + 1) % num2;
			int num15 = PathData.DungeonIndices[num14];
			BeginMovePath(new int[1] { num15 }, num15);
		}
	}

	protected override void OnMoved()
	{
		base.OnMoved();
		int num = __Extension_Method_ICollection__.COUNT(PathData.DungeonIndices);
		PathNo = (PathNo + 1) % num;
		SaveSpotNow();
		if (PathData != null && __Extension_Method_ICollection__.COUNT(PathData.DungeonIndices) != 0)
		{
			int num2 = (PathNo + 1) % num;
			int num3 = PathData.DungeonIndices[num2];
			BeginMovePath(new int[1] { num3 }, num3);
		}
	}

	private void SaveSpotNow()
	{
		_lastPathNo.Value = PathNo;
		_lastPathDate.Value = NGame2.NUI.NWindow.Common.GetUTCNow();
	}
}
