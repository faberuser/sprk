using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NodeGraph
{
	private Mediator _mediator;

	private Transform _nodeRoot;

	private IncreaseDic<int, MapNode> _nodeDictionary = new IncreaseDic<int, MapNode>();

	private IncreaseDic<Tuple<int, int>, MapEdge2> _edgeDictionary = new IncreaseDic<Tuple<int, int>, MapEdge2>();

	public IncreaseDic<int, MapNode> GetNodeDictionary()
	{
		return _nodeDictionary;
	}

	public MapNode GetNodeByDungeonIndex(int dungeonIndex)
	{
		return _nodeDictionary.GETVALUE(dungeonIndex, null);
	}

	public MapEdge2 GetEdgeByDungeonIndex(int dungeon1, int dungeon2)
	{
		return _edgeDictionary.GETVALUE(new Tuple<int, int>(dungeon1, dungeon2), null);
	}

	public MapNode GetNodeByLinkedDungeonIndex(int linkedDungeonIndex)
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(i);
			if (valueByIndex.LinkedDungeonIndex == linkedDungeonIndex)
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public Vector3 GetNodePositionByDungeonIndex(int dungeonIndex)
	{
		MapNode nodeByDungeonIndex = GetNodeByDungeonIndex(dungeonIndex);
		if (nodeByDungeonIndex != null)
		{
			return nodeByDungeonIndex.transform.localPosition;
		}
		VespaDebug.LogError("Node not found : " + dungeonIndex);
		return Vector3.zero;
	}

	public MapNode FindStartNode()
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(i);
			if (valueByIndex.PrevNodes.Count == 0)
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public void Open(Mediator mediator, Transform nodeRoot)
	{
		_mediator = mediator;
		_nodeRoot = nodeRoot;
	}

	public void Build(CampaignChapterData chapterData, Action<string> onMessage = null)
	{
		Vespa_System_Action_Extension_Method.fire(onMessage, "ClearNodeGraph");
		ClearNodeGraph();
		if (chapterData == null)
		{
			return;
		}
		int index = chapterData.Index;
		if (chapterData.LabelScale > 0)
		{
			_ = chapterData.LabelScale;
		}
		HashSet<int> hashSet = new HashSet<int>();
		Vespa_System_Action_Extension_Method.fire(onMessage, "firstDungeonIndexSet");
		IList<CampaignNodeData> allChapterData = GameRoot.Instance.CampaignNodeDataContainer.GetAllChapterData(index);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allChapterData); i++)
		{
			CampaignNodeData campaignNodeData = allChapterData[i];
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(campaignNodeData.ChapterIndex, campaignNodeData.DungeonIndex);
			if (data != null && data.FirstDungeonIndex > 0)
			{
				hashSet.Add(data.FirstDungeonIndex);
			}
		}
		Vespa_System_Action_Extension_Method.fire(onMessage, "CreateNode");
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(allChapterData); j++)
		{
			CampaignNodeData campaignNodeData2 = allChapterData[j];
			int dungeonIndex = campaignNodeData2.DungeonIndex;
			if (!hashSet.Contains(dungeonIndex))
			{
				CreateNode(chapterData, campaignNodeData2);
			}
		}
		Vespa_System_Action_Extension_Method.fire(onMessage, "ConstructNodeDependency");
		ConstructNodeDependency();
		Vespa_System_Action_Extension_Method.fire(onMessage, "CreateEdgeToNextNode");
		for (int k = 0; k < _nodeDictionary.Count; k++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(k);
			CreateEdgeToNextNode(chapterData, valueByIndex);
		}
	}

	public void ClearNodeGraph()
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			FactoryBase.ReleaseComponent(_nodeDictionary.GetValueByIndex(i));
		}
		_nodeDictionary.Clear();
		for (int j = 0; j < _edgeDictionary.Count; j++)
		{
			FactoryBase.ReleaseComponent(_edgeDictionary.GetValueByIndex(j));
		}
		_edgeDictionary.Clear();
	}

	public void Show(bool show)
	{
		UIHelper.EnableGameObject(_nodeRoot, show);
	}

	public void RecreateNode(int dungeonIndex)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		MapNode nodeByDungeonIndex = GetNodeByDungeonIndex(dungeonIndex);
		if (nodeByDungeonIndex == null)
		{
			return;
		}
		List<MapNode> list = new List<MapNode>();
		list.Add(nodeByDungeonIndex);
		FindNearbyNodeList(nodeByDungeonIndex, list);
		List<MapNode> list2 = new List<MapNode>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			MapNode mapNode = list[i];
			int dungeonIndex2 = mapNode.DungeonIndex;
			CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex2);
			_nodeDictionary.RemoveSlow(dungeonIndex2);
			FactoryBase.ReleaseComponent(mapNode);
			MapNode item = CreateNode(data, data2);
			list2.Add(item);
		}
		nodeByDungeonIndex = GetNodeByDungeonIndex(dungeonIndex);
		if (nodeByDungeonIndex == null)
		{
			VespaDebug.LogError("Cannot find recreated node : dungeonIndex=" + dungeonIndex);
			return;
		}
		ConstructNodeDependency();
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(nodeByDungeonIndex.PrevNodes); j++)
		{
			MapNode nodeComponent = nodeByDungeonIndex.PrevNodes[j];
			CreateEdgeToNextNode(data, nodeComponent);
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(list2); k++)
		{
			MapNode nodeComponent2 = list2[k];
			CreateEdgeToNextNode(data, nodeComponent2);
		}
	}

	public void RemoveNode(int dungeonIndex)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		MapNode nodeByDungeonIndex = GetNodeByDungeonIndex(dungeonIndex);
		if (!(nodeByDungeonIndex == null))
		{
			_nodeDictionary.RemoveSlow(dungeonIndex);
			FactoryBase.ReleaseComponent(nodeByDungeonIndex);
			ConstructNodeDependency();
		}
	}

	private void FindNearbyNodeList(MapNode mapNode, List<MapNode> nearbyNodeList)
	{
		if (mapNode == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mapNode.NextNodes); i++)
		{
			MapNode mapNode2 = mapNode.NextNodes[i];
			if (!nearbyNodeList.Contains(mapNode2))
			{
				nearbyNodeList.Add(mapNode2);
				if (!mapNode2.Function.IsBattleFunction())
				{
					FindNearbyNodeList(mapNode2, nearbyNodeList);
				}
			}
		}
	}

	private void CreateNode(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		CampaignChapterData data2 = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (CreateNode(data2, data) != null)
		{
			RecreateNode(dungeonIndex);
		}
	}

	private MapNode CreateNode(CampaignChapterData chapterData, CampaignNodeData nodeData)
	{
		if (chapterData == null || nodeData == null)
		{
			return null;
		}
		if (chapterData.Index != NGame2.NAccount.UserCampaignManager.ChapterIndex)
		{
			VespaDebug.LogError("Cannot create other chapter node. inputChapter=" + chapterData.Index + " current=" + NGame2.NAccount.UserCampaignManager.ChapterIndex);
			return null;
		}
		if (chapterData.Index != nodeData.ChapterIndex)
		{
			VespaDebug.LogError("chapterIndex of chapterData and nodeData mismatch. chapterData=" + chapterData.Index + " nodeData=" + nodeData.ChapterIndex);
			return null;
		}
		if (nodeData.Hidden && !Helper.CheckDropBonusEventNode(nodeData.ChapterIndex, nodeData.DungeonIndex))
		{
			return null;
		}
		int index = chapterData.Index;
		int dungeonIndex = nodeData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(index, dungeonIndex);
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(index, dungeonIndex);
		int num = data?.FirstDungeonIndex ?? 0;
		if (num > 0 && !flag)
		{
			nodeData = GameRoot.Instance.CampaignNodeDataContainer.GetData(index, num);
			if (nodeData == null)
			{
				VespaDebug.LogError("Failed to create first dungeon node : dungeonIndex=" + dungeonIndex);
				return null;
			}
		}
		PrefabAuto<MapNode> prefabAuto = MapNode.GetPrefabAuto(nodeData.NodePrefab);
		if (prefabAuto == null || prefabAuto.Factory == null)
		{
			VespaDebug.LogError("Failed to get mapnode prefab auto : " + nodeData.NodePrefab);
			return null;
		}
		MapNode mapNode = prefabAuto.Factory.Create(_nodeRoot);
		if (mapNode == null)
		{
			VespaDebug.LogError("Failed to create mapnode: " + nodeData.NodePrefab);
			return null;
		}
		mapNode.Open(_nodeRoot.gameObject.layer, dungeonIndex, nodeData, _mediator.ConvertNodePosition(nodeData.UiPositionX, nodeData.UiPositionY), new Vector3(_mediator.MapComponent.ScaleData.NodeScaleX, _mediator.MapComponent.ScaleData.NodeScaleY, 1f), OnTouchNode, OnDragNode, OnDragStartNode, OnDragEndNode, OnLongTouchNode);
		if (chapterData.Type == ChapterType.WorldMap && !mapNode.CanVisit)
		{
			UIHelper.EnableGameObject(mapNode, enable: false);
		}
		else if (GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNodeReserved(index, dungeonIndex))
		{
			UIHelper.EnableGameObject(mapNode, enable: false);
		}
		_nodeDictionary.Add(dungeonIndex, mapNode);
		return mapNode;
	}

	private void CreateEdgeToNextNode(CampaignChapterData chapterData, MapNode nodeComponent)
	{
		if (nodeComponent == null || nodeComponent.NodeData == null || string.IsNullOrEmpty(nodeComponent.NodeData.NodePrefab))
		{
			return;
		}
		bool canBypass = nodeComponent.CanBypass;
		for (int i = 0; i < nodeComponent.NextNodes.Count; i++)
		{
			MapNode mapNode = nodeComponent.NextNodes[i];
			if (mapNode.NodeData == null || string.IsNullOrEmpty(mapNode.NodeData.NodePrefab))
			{
				continue;
			}
			MapEdge2 mapEdge = CreateEdgeComponent(nodeComponent, mapNode);
			if (!(mapEdge == null))
			{
				MapEdge2 o_value = null;
				Tuple<int, int> key = new Tuple<int, int>(nodeComponent.DungeonIndex, mapNode.DungeonIndex);
				if (_edgeDictionary.TryGetValue(key, out o_value))
				{
					_edgeDictionary.RemoveSlow(key);
					FactoryBase.SafeReleaseComponent(ref o_value);
				}
				if (chapterData.Type == ChapterType.WorldMap && (!mapNode.CanVisit || !canBypass))
				{
					UIHelper.EnableGameObject(mapEdge, enable: false);
				}
				else if (GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckEdgeReserved(nodeComponent.ChapterIndex, nodeComponent.DungeonIndex, mapNode.DungeonIndex))
				{
					UIHelper.EnableGameObject(mapEdge, enable: false);
				}
				_edgeDictionary.Add(key, mapEdge);
			}
		}
		foreach (MapNode oneWayNode in nodeComponent.OneWayNodes)
		{
			MapEdge2 mapEdge2 = CreateEdgeComponent(nodeComponent, oneWayNode);
			if (!(mapEdge2 == null))
			{
				MapEdge2 o_value2 = null;
				Tuple<int, int> key2 = new Tuple<int, int>(nodeComponent.DungeonIndex, oneWayNode.DungeonIndex);
				if (_edgeDictionary.TryGetValue(key2, out o_value2))
				{
					_edgeDictionary.RemoveSlow(key2);
					FactoryBase.SafeReleaseComponent(ref o_value2);
				}
				if (chapterData.Type == ChapterType.WorldMap && (!oneWayNode.CanVisit || !canBypass))
				{
					UIHelper.EnableGameObject(mapEdge2, enable: false);
				}
				else if (GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckEdgeReserved(nodeComponent.ChapterIndex, nodeComponent.DungeonIndex, oneWayNode.DungeonIndex))
				{
					UIHelper.EnableGameObject(mapEdge2, enable: false);
				}
				_edgeDictionary.Add(key2, mapEdge2);
			}
		}
	}

	private MapEdge2 CreateEdgeComponent(Vector3 localStartPosition, Vector3 localEndPosition, bool leftBridge, bool rightBridge)
	{
		MapEdge2 mapEdge = MapEdge2.PrefabAuto.Factory.Create(_nodeRoot);
		mapEdge.Open(localStartPosition, localEndPosition, leftBridge, rightBridge);
		return mapEdge;
	}

	private MapEdge2 CreateEdgeComponent(MapNode fromNode, MapNode toNode)
	{
		if (fromNode == null)
		{
			return null;
		}
		if (toNode == null)
		{
			return null;
		}
		_ = toNode.DungeonIndex;
		Vector3 localPosition = fromNode.transform.localPosition;
		Vector3 localPosition2 = toNode.transform.localPosition;
		return CreateEdgeComponent(localPosition, localPosition2, fromNode.NodeData.Function == NodeFunctionType.None, toNode.NodeData.Function == NodeFunctionType.None);
	}

	private void ConstructNodeDependency()
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(i);
			valueByIndex.PrevNodes.Clear();
			valueByIndex.NextNodes.Clear();
		}
		for (int j = 0; j < _nodeDictionary.Count; j++)
		{
			MapNode valueByIndex2 = _nodeDictionary.GetValueByIndex(j);
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(valueByIndex2.ChapterIndex, valueByIndex2.DungeonIndex);
			if (data == null)
			{
				continue;
			}
			int[] nextNodeIndex = data.NextNodeIndex;
			if (nextNodeIndex == null || nextNodeIndex.Length == 0)
			{
				continue;
			}
			foreach (int key in nextNodeIndex)
			{
				MapNode o_value = null;
				_nodeDictionary.TryGetValue(key, out o_value);
				if (!(o_value == null))
				{
					o_value.PrevNodes.Add(valueByIndex2);
					valueByIndex2.NextNodes.Add(o_value);
				}
			}
		}
	}

	private void OnTouchNode(MapNode nodeComponent)
	{
		_mediator.AvatarController.OnTouchNode(nodeComponent);
	}

	private void OnLongTouchNode(MapNode nodeComponent)
	{
		_mediator.OnLongTouch(nodeComponent);
		if (_mediator.Viewer != null)
		{
			_mediator.Viewer.OnDragEndWorldMap();
		}
	}

	private void OnDragNode(MapNode nodeComponent, Vector2 delta)
	{
		if (_mediator.Viewer != null)
		{
			_mediator.Viewer.OnDragWorldMap(delta);
		}
	}

	private void OnDragStartNode(MapNode nodeComponent)
	{
		if (_mediator.Viewer != null)
		{
			_mediator.Viewer.OnDragStartWorldMap();
		}
	}

	private void OnDragEndNode(MapNode nodeComponent)
	{
		if (_mediator.Viewer != null)
		{
			_mediator.Viewer.OnDragEndWorldMap();
		}
	}

	public void OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAddedNode)
	{
		if (isAddedNode)
		{
			CreateNode(chapterIndex, dungeonIndex);
		}
		else
		{
			RemoveNode(dungeonIndex);
		}
	}
}
