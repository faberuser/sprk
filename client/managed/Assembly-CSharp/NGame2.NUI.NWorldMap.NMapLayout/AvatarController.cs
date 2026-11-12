using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NContents;
using NGame2.NOrdeal;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGameAsset.NWorldMap;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class AvatarController
{
	public interface IListener
	{
		int GetChapterIndex();

		int GetDungeonIndex();

		void SetDungeonIndex(int dungeonIndex);

		Avatar GetAvatar();

		NodeGraph GetNodeGraph();

		ScaleData GetScaleData();

		void OnMoveBegin(int chapterIndex, int[] dungeonIndices);

		void OnMoveEnd();

		void OnVisitNode(int chapterIndex, int dungeonIndex);
	}

	public enum EUpdateMode
	{
		Straight,
		Roaming
	}

	public float SpeedFactor = 1f;

	protected IListener _listener;

	protected List<int> _moveDungeonIndexList = new List<int>();

	private List<object> _moveLockerList = new List<object>();

	private Vector3 _nextDungeonPosition;

	private List<IListener> _iAiAvatarControllerListenerList = new List<IListener>();

	public int LastDungeonIndex { get; private set; }

	public AvatarController(IListener listener)
	{
		_listener = listener;
		LastDungeonIndex = listener.GetDungeonIndex();
		_iAiAvatarControllerListenerList.Clear();
	}

	public void SetAiAvatarControllerListener(IListener iAiAvatarControllerListener)
	{
		if (iAiAvatarControllerListener != null && !_iAiAvatarControllerListenerList.Contains(iAiAvatarControllerListener))
		{
			_iAiAvatarControllerListenerList.Add(iAiAvatarControllerListener);
		}
	}

	public int GetDungeonIndex()
	{
		return _listener.GetDungeonIndex();
	}

	public void AddTouchLocker(object locker)
	{
		if (!_moveLockerList.Contains(locker))
		{
			_moveLockerList.Add(locker);
		}
	}

	public void RemoveTouchLocker(object locker)
	{
		_moveLockerList.Remove(locker);
	}

	public void OnUnloadMap()
	{
		_moveDungeonIndexList.Clear();
	}

	public void OnLoadMap()
	{
		_moveDungeonIndexList.Clear();
		SetAvatarNextDirection();
		OnVisited(LastDungeonIndex);
	}

	public int[] OnTouchNode(MapNode node, bool userInput = true)
	{
		if (node == null)
		{
			return null;
		}
		if (_listener.GetAvatar() == null)
		{
			return null;
		}
		if (_moveLockerList.Count > 0)
		{
			return null;
		}
		if (!SingletonAuto<ContentsOpenChecker>.instance.CheckNodeOpen(node.ChapterIndex, node.DungeonIndex))
		{
			SceneAutoBehaviour<MissionManagement>.instance.OpenCenterMessageMainQuestConditionFail();
			return null;
		}
		if (!node.CanVisit)
		{
			if (node.IsOrdealArenaNode)
			{
				int chapterIndex = node.ChapterIndex;
				int dungeonIndex = node.DungeonIndex;
				OrdealArenaNodeData data = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
				if (data == null)
				{
					return null;
				}
				int completeFloor = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetCompleteFloor();
				int floor = data.Floor;
				if (completeFloor >= floor)
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("ORDEALARENA_NODE_ALREADY_CLEARED_FLOOR"));
					return null;
				}
				if (completeFloor + 1 == floor)
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("ORDEALARENA_NODE_UNREACHABLE_ROUTE"));
					return null;
				}
			}
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return null;
		}
		if (userInput)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.SetOnPlayerMoveEnd(null);
		}
		return BeginMoveTo(node.DungeonIndex, userInput);
	}

	public int[] BeginMoveTo(int targetDungeonIndex, bool userInput)
	{
		if (_listener.GetDungeonIndex() == targetDungeonIndex && userInput)
		{
			if (!IsAvatarMoving())
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
			}
			return null;
		}
		int[] array = FindMinPathTo(_listener.GetDungeonIndex(), targetDungeonIndex);
		if (array == null)
		{
			return null;
		}
		BeginMovePath(array, targetDungeonIndex);
		return array;
	}

	public void BeginMovePath(int[] dungeonIndices, int targetDungeonIndex)
	{
		AddMovePath(dungeonIndices);
		_listener.OnMoveBegin(_listener.GetChapterIndex(), dungeonIndices);
		AiAvatarBeginMove(dungeonIndices, targetDungeonIndex);
	}

	private void AiAvatarBeginMove(int[] dungeonIndices, int targetDungeonIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_iAiAvatarControllerListenerList))
		{
			return;
		}
		_iAiAvatarControllerListenerList.ForEach(delegate(IListener iListener)
		{
			if (iListener != null)
			{
				dungeonIndices = FindMinPathTo(iListener.GetDungeonIndex(), targetDungeonIndex);
				iListener.OnMoveBegin(iListener.GetChapterIndex(), dungeonIndices);
			}
		});
	}

	public void AddMovePath(int[] dungeonIndices)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dungeonIndices))
		{
			_moveDungeonIndexList.Clear();
			_moveDungeonIndexList.AddRange(dungeonIndices);
		}
	}

	public void WarpTo(int targetDungeonIndex)
	{
		BeginMovePath(new int[1] { targetDungeonIndex }, targetDungeonIndex);
		OnVisited(targetDungeonIndex);
		Vector3 nodePositionByDungeonIndex = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(targetDungeonIndex);
		_listener.GetAvatar().SetPosition(nodePositionByDungeonIndex);
		_listener.SetDungeonIndex(targetDungeonIndex);
		LastDungeonIndex = targetDungeonIndex;
		_moveDungeonIndexList.Clear();
		OnMoved();
	}

	public void SetNextDungeonPosition(Vector3 position)
	{
		_nextDungeonPosition = position;
	}

	public void ForceDirection(Vector2 dir)
	{
		SetNextDungeonPosition(_listener.GetAvatar().GetLocalPosition() + Vespa_Unity_Vector2_Extension_Method.ToVec3(dir));
		SetAiAvatarNextDungeonPosition(dir);
	}

	private void SetAiAvatarNextDungeonPosition(Vector2 dir)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_iAiAvatarControllerListenerList))
		{
			return;
		}
		_iAiAvatarControllerListenerList.ForEach(delegate(IListener iListener)
		{
			if (iListener != null && iListener.GetAvatar() != null)
			{
				iListener.GetAvatar().SetForcedDirection(dir);
			}
		});
	}

	protected virtual void OnMoved()
	{
		_listener.OnMoveEnd();
		_nextDungeonPosition = _listener.GetAvatar().GetLocalPosition();
	}

	protected virtual void OnVisited(int dungeonIndex)
	{
		_listener.OnVisitNode(_listener.GetChapterIndex(), dungeonIndex);
	}

	public float GetSpeed()
	{
		return _listener.GetScaleData().PlayerMoveSpeed * SpeedFactor;
	}

	public bool IsAvatarMoving()
	{
		return _moveDungeonIndexList.Count > 0;
	}

	public bool IsAvatarMoving(out int destinationIndex)
	{
		if (_moveDungeonIndexList.Count > 0)
		{
			destinationIndex = _moveDungeonIndexList[_moveDungeonIndexList.Count - 1];
			return true;
		}
		destinationIndex = 0;
		return false;
	}

	public virtual void ChainUpdate()
	{
		float deltaTime = MainTimer.deltaTime;
		if (IsAvatarMoving())
		{
			UpdateAvatarMoving(deltaTime);
		}
		else
		{
			UpdateAvatarIdleDirection(deltaTime);
		}
		UpdateAnimation(deltaTime);
	}

	public void UpdateAvatarMoving(float deltaTime)
	{
		if (_moveDungeonIndexList.Count == 0)
		{
			return;
		}
		int num = _moveDungeonIndexList[0];
		_listener.SetDungeonIndex(num);
		Vector3 localPosition = _listener.GetAvatar().GetLocalPosition();
		Vector3 nodePositionByDungeonIndex = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(num);
		Vector3 vector = nodePositionByDungeonIndex - localPosition;
		if (vector.magnitude > 0f && GetSpeed() > 0f)
		{
			float direction = NVespa.Math.Angle(Vespa_Unity_Vector3_Extension_Method.XY(vector.normalized));
			_listener.GetAvatar().SetDirection(direction);
			float maxDistanceDelta = deltaTime * GetSpeed();
			Vector3 vector2 = Vector3.MoveTowards(localPosition, nodePositionByDungeonIndex, maxDistanceDelta);
			_listener.GetAvatar().SetPosition(vector2);
			if (Vector3.Distance(vector2, nodePositionByDungeonIndex) > Mathf.Epsilon)
			{
				return;
			}
		}
		_moveDungeonIndexList.RemoveAt(0);
		LastDungeonIndex = num;
		OnVisited(num);
		if (_moveDungeonIndexList.Count == 0)
		{
			_listener.GetAvatar().SetPosition(nodePositionByDungeonIndex);
			OnMoved();
		}
	}

	public void SetAvatarNextDirection()
	{
		Vector3 localPosition = _listener.GetAvatar().GetLocalPosition();
		int lastDungeonIndex = LastDungeonIndex;
		List<int> list = new List<int>();
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(_listener.GetChapterIndex(), lastDungeonIndex);
		if (data != null && __Extension_Method_ICollection__.COUNT(data.NextNodeIndex) > 0)
		{
			list.AddRange(data.NextNodeIndex);
		}
		HashSet<int> prevDungeonIndices = GameRoot.Instance.CampaignNodeDataContainer.GetPrevDungeonIndices(_listener.GetChapterIndex(), lastDungeonIndex);
		if (__Extension_Method_ICollection__.COUNT(prevDungeonIndices) > 0)
		{
			list.AddRange(prevDungeonIndices);
		}
		if (list.Count > 0)
		{
			float num = float.MaxValue;
			Vector3 nextDungeonPosition = localPosition;
			for (int i = 0; i < list.Count; i++)
			{
				int dungeonIndex = list[i];
				Vector3 nodePositionByDungeonIndex = _listener.GetNodeGraph().GetNodePositionByDungeonIndex(dungeonIndex);
				Vector3 to = nodePositionByDungeonIndex - _listener.GetAvatar().GetLocalPosition();
				if (!Mathf.Approximately(to.magnitude, 0f))
				{
					float num2 = Vector3.Angle(_listener.GetAvatar().CurrentDirection, to);
					if (num2 < num)
					{
						nextDungeonPosition = nodePositionByDungeonIndex;
						num = num2;
					}
				}
			}
			_nextDungeonPosition = nextDungeonPosition;
		}
		else
		{
			_nextDungeonPosition = localPosition;
		}
	}

	public virtual void UpdateAvatarIdleDirection(float deltaTime)
	{
		Vector3 vector = _nextDungeonPosition - _listener.GetAvatar().GetLocalPosition();
		vector.z = 0f;
		if (!Mathf.Approximately(vector.x, 0f) || !Mathf.Approximately(vector.y, 0f))
		{
			_listener.GetAvatar().SetDirection(NVespa.Math.Angle(Vespa_Unity_Vector3_Extension_Method.XY(vector.normalized)));
		}
	}

	private void LerpAvatarDirection(float deltaTime, Vector3 targetDirection)
	{
		float num = Vector3.Angle(_listener.GetAvatar().CurrentDirection, targetDirection) * (MathF.PI / 180f);
		Vector3 i_src = Vector3.RotateTowards(_listener.GetAvatar().CurrentDirection, targetDirection, num * deltaTime * 7f, 0f);
		if (i_src.magnitude > 0f)
		{
			_listener.GetAvatar().SetDirection(NVespa.Math.Angle(Vespa_Unity_Vector3_Extension_Method.XY(i_src)));
		}
	}

	public void UpdateAnimation(float deltaTime)
	{
		_listener.GetAvatar()?.SetAnimation(IsAvatarMoving());
	}

	private void FindPathTo(MapNode mapNode, int toDungeonIndex, LinkedList<int> dungeonIndexList, Action<int[]> onFound)
	{
		if (mapNode == null)
		{
			return;
		}
		int dungeonIndex = mapNode.DungeonIndex;
		if (dungeonIndex == toDungeonIndex)
		{
			if (onFound != null)
			{
				dungeonIndexList.AddLast(toDungeonIndex);
				int[] obj = dungeonIndexList.ToArray();
				dungeonIndexList.RemoveLast();
				onFound(obj);
			}
			return;
		}
		dungeonIndexList.AddLast(dungeonIndex);
		List<MapNode> list = new List<MapNode>();
		list.AddRange(mapNode.NextNodes);
		list.AddRange(mapNode.PrevNodes);
		foreach (MapNode item in list)
		{
			if (item.CanVisit && !dungeonIndexList.Contains(item.DungeonIndex))
			{
				FindPathTo(item, toDungeonIndex, dungeonIndexList, onFound);
			}
		}
		dungeonIndexList.RemoveLast();
	}

	public int[] FindMinPathTo(int fromDungeonIndex, int toDungeonIndex)
	{
		NodeGraph nodeGraph = _listener.GetNodeGraph();
		IncreaseDic<int, MapNode> nodeDictionary = nodeGraph.GetNodeDictionary();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(nodeDictionary); i++)
		{
			MapNode valueByIndex = nodeDictionary.GetValueByIndex(i);
			dictionary2[valueByIndex.DungeonIndex] = int.MaxValue;
		}
		dictionary[fromDungeonIndex] = fromDungeonIndex;
		dictionary2[fromDungeonIndex] = 0;
		Queue<MapNode> queue = new Queue<MapNode>();
		queue.Enqueue(nodeGraph.GetNodeByDungeonIndex(fromDungeonIndex));
		while (queue.Count > 0)
		{
			MapNode mapNode = queue.Dequeue();
			if (mapNode == null)
			{
				continue;
			}
			int dungeonIndex = mapNode.DungeonIndex;
			if (dungeonIndex == toDungeonIndex)
			{
				List<int> list = new List<int>();
				for (int num = toDungeonIndex; num != fromDungeonIndex; num = dictionary[num])
				{
					list.Add(num);
				}
				list.Add(fromDungeonIndex);
				list.Reverse();
				return list.ToArray();
			}
			int num2 = dictionary2[dungeonIndex] + 1;
			List<MapNode> list2 = new List<MapNode>();
			if (mapNode.CanBypass)
			{
				list2.AddRange(mapNode.NextNodes);
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(mapNode.PrevNodes); j++)
			{
				MapNode mapNode2 = mapNode.PrevNodes[j];
				if (mapNode2.CanBypass)
				{
					list2.Add(mapNode2);
				}
			}
			foreach (MapNode item in list2)
			{
				if (!item.CanVisit)
				{
					continue;
				}
				int dungeonIndex2 = item.DungeonIndex;
				if (dictionary2[dungeonIndex2] > num2)
				{
					dictionary[dungeonIndex2] = dungeonIndex;
					dictionary2[dungeonIndex2] = num2;
					if (!queue.Contains(item))
					{
						queue.Enqueue(item);
					}
				}
			}
		}
		return null;
	}
}
