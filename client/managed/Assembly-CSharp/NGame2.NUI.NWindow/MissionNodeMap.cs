using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[RequireComponent(typeof(UIWidget))]
public class MissionNodeMap : MonoBehaviour, CommonNode.IListner
{
	[Serializable]
	public class NodeMapPositionInfo
	{
		public Vector2 paddingSize;

		public Vector2 offsetSize;

		public Vector2 componentSize;

		public float nodeScale;

		public Vector2 GetRealCompSize()
		{
			return componentSize * nodeScale;
		}

		public Vector2 GetRealPadding()
		{
			return paddingSize * nodeScale;
		}
	}

	public interface IListner
	{
		void ScrollViewCustomMovement(int x, int y);

		void OnClick(MissionComponentData data);
	}

	public const int CHECK_STEP_SCROLLVIEW_MOVEMENT = 3;

	public const int LIMITED_WIDTH_SCROLLVIEW_NODE_CUONT = 6;

	[Header("[MissionNodeMap]")]
	public NodeMapPositionInfo[] nodeMapPositionInfos;

	private IListner _listener;

	private NodeMapPositionInfo _selectMapPosition;

	private List<MissionNodeComponent> _nodeComponents = new List<MissionNodeComponent>();

	private List<Tuple<Type, int>> _listClearIndex = new List<Tuple<Type, int>>();

	private Vector2 _awakenNodeLeastPosition = Vector2.zero;

	private Vector2 _awakenRewardNodeLeastPosition = Vector2.zero;

	public int SelectedMainCategory { get; set; }

	public int SelectedSubCategory { get; set; }

	void CommonNode.IListner.OnClick(MissionComponentData data)
	{
		if (_listener != null)
		{
			_listener.OnClick(data);
		}
	}

	public void CreateMap(IListner listner, int mainCategory, int subCategory)
	{
		_listener = listner;
		List<MissionVisualData> missionVisualDataList = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(mainCategory, subCategory);
		if (missionVisualDataList == null || __Extension_Method_ICollection__.COUNT(missionVisualDataList) == 0)
		{
			return;
		}
		List<MissionComponentData> list = new List<MissionComponentData>();
		int num = 0;
		int num2 = 0;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (MissionVisualData item4 in missionVisualDataList)
		{
			string questType = item4.QuestType;
			int questIndex = item4.QuestIndex;
			if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
			{
				if (item4.MainStep > 0 && item4.SubStep == 0)
				{
					MissionComponentData item = new MissionComponentData(item4);
					list.Add(item);
				}
			}
			else if (questType.CompareTo("Achievement") == 0)
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(questIndex, 1);
				if (data == null)
				{
					VespaDebug.LogError("MissionNodeMap AchievementData is null. " + questIndex);
					continue;
				}
				MissionComponentData item2 = new MissionComponentData(data, item4);
				list.Add(item2);
				if (num < item4.SubStep)
				{
					num = item4.SubStep;
				}
				if (num2 < item4.MainStep)
				{
					num2 = item4.MainStep;
				}
				if (dictionary.ContainsKey(item4.MainStep))
				{
					if (dictionary[item4.MainStep] < item4.SubStep)
					{
						dictionary[item4.MainStep] = item4.SubStep;
					}
				}
				else
				{
					dictionary.Add(item4.MainStep, item4.SubStep);
				}
			}
			else
			{
				if (questType.CompareTo("SubQuest") != 0)
				{
					continue;
				}
				SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(questIndex);
				if (subQuestData == null)
				{
					VespaDebug.LogError("MissionNodeMap SubQuestData is null. " + questIndex);
					continue;
				}
				MissionComponentData item3 = new MissionComponentData(subQuestData, item4);
				list.Add(item3);
				if (num < item4.SubStep)
				{
					num = item4.SubStep;
				}
				if (num2 < item4.MainStep)
				{
					num2 = item4.MainStep;
				}
				if (dictionary.ContainsKey(item4.MainStep))
				{
					if (dictionary[item4.MainStep] < item4.SubStep)
					{
						dictionary[item4.MainStep] = item4.SubStep;
					}
				}
				else
				{
					dictionary.Add(item4.MainStep, item4.SubStep);
				}
			}
		}
		int num3 = num2 - 1;
		if (num3 >= 0 && __Extension_Method_ICollection__.COUNT(nodeMapPositionInfos) > num3)
		{
			_selectMapPosition = nodeMapPositionInfos[num3];
		}
		else
		{
			_selectMapPosition = Vespa_Generic_IList_Extension_Method.GetLast(nodeMapPositionInfos);
		}
		float num4 = (float)num * _selectMapPosition.GetRealCompSize().x + (float)num * _selectMapPosition.GetRealPadding().x;
		if (num < 6)
		{
			num4 = 6f * _selectMapPosition.GetRealCompSize().x + 6f * _selectMapPosition.GetRealPadding().x;
		}
		foreach (MissionComponentData item5 in list)
		{
			if (item5 != null && item5.VisualData != null)
			{
				int mainStep = item5.VisualData.MainStep;
				int subStep = item5.VisualData.SubStep;
				int num5 = dictionary[mainStep];
				if (num5 <= 0)
				{
					num5 = 1;
				}
				float x = _selectMapPosition.offsetSize.x + (float)subStep * (num4 / (float)num5);
				float y = _selectMapPosition.offsetSize.y - (float)(mainStep - 1) * _selectMapPosition.GetRealCompSize().y - (float)(mainStep - 1) * _selectMapPosition.GetRealPadding().y;
				Vector2 position = new Vector2(x, y);
				Vector2 scale = Vector2.one * _selectMapPosition.nodeScale;
				CreateNodeComponent(item5, mainStep, subStep, position, scale);
			}
		}
		if (_listener != null)
		{
			_listener.ScrollViewCustomMovement(1, (num2 > 3) ? 1 : 0);
		}
	}

	private void CreateNodeComponent(MissionComponentData data, int mainStep, int subStep, Vector2 position, Vector2 scale)
	{
		if (data == null || data.VisualData == null)
		{
			return;
		}
		MissionNodeComponent missionNodeComponent = null;
		MissionNodeComponent.ENodeType eNodeType = MissionNodeComponent.ENodeType.Node;
		if (subStep == 0)
		{
			missionNodeComponent = MissionNodeComponent.PrefabAuto4.Factory.Create(base.transform, base.gameObject.layer);
			eNodeType = MissionNodeComponent.ENodeType.StartNode;
		}
		else
		{
			switch (EnumTools.Parse(GameRoot.Instance.MissionCategoryDataContainer.GetData(data.VisualData.MainCategoryIndex, (data.VisualData.SubCategoryIndex == 0) ? 1 : data.VisualData.SubCategoryIndex).UIPropName, EUIPropType.Null))
			{
			case EUIPropType.Node:
				missionNodeComponent = MissionNodeComponent.PrefabAuto.Factory.Create(base.transform, base.gameObject.layer);
				eNodeType = MissionNodeComponent.ENodeType.Node;
				break;
			case EUIPropType.Node3:
				missionNodeComponent = MissionNodeComponent.PrefabAuto5.Factory.Create(base.transform, base.gameObject.layer);
				eNodeType = MissionNodeComponent.ENodeType.Node3;
				break;
			}
		}
		missionNodeComponent.transform.localScale = Vector3.one;
		missionNodeComponent.Open(data, this, mainStep, subStep, position, scale, eNodeType);
		if (subStep != 0)
		{
			MissionNodeComponent last = Vespa_Generic_IList_Extension_Method.GetLast(_nodeComponents);
			missionNodeComponent.DrawLinkedLine(last.GetPosition());
		}
		if (missionNodeComponent.IsRewardAble())
		{
			_listClearIndex.Add(new Tuple<Type, int>(data.DataType, data.Index));
			if (_awakenRewardNodeLeastPosition == Vector2.zero || _awakenRewardNodeLeastPosition.x > position.x)
			{
				_awakenRewardNodeLeastPosition = position;
			}
		}
		if (missionNodeComponent.IsProgress() && (_awakenNodeLeastPosition == Vector2.zero || _awakenNodeLeastPosition.x > position.x))
		{
			_awakenNodeLeastPosition = position;
		}
		_nodeComponents.Add(missionNodeComponent);
	}

	public void Refresh()
	{
		foreach (MissionNodeComponent nodeComponent in _nodeComponents)
		{
			nodeComponent.Refresh();
		}
		_listClearIndex.Clear();
		foreach (MissionNodeComponent nodeComponent2 in _nodeComponents)
		{
			if (nodeComponent2.IsRewardAble())
			{
				MissionComponentData elementData = nodeComponent2.ElementData;
				if (elementData != null)
				{
					_listClearIndex.Add(new Tuple<Type, int>(elementData.DataType, elementData.Index));
				}
			}
		}
	}

	public void Clear()
	{
		_awakenNodeLeastPosition = Vector2.zero;
		_awakenRewardNodeLeastPosition = Vector2.zero;
		if (_nodeComponents != null && __Extension_Method_ICollection__.COUNT(_nodeComponents) > 0)
		{
			foreach (MissionNodeComponent nodeComponent in _nodeComponents)
			{
				nodeComponent.Clear();
			}
			FactoryBase.ReleaseAndClearList(_nodeComponents);
		}
		_nodeComponents.Clear();
		_listClearIndex.Clear();
	}

	public List<int> GetAchievementRewardList()
	{
		List<int> list = new List<int>();
		foreach (Tuple<Type, int> item in _listClearIndex)
		{
			if (item.Item1 == typeof(AchievementData))
			{
				list.Add(item.Item2);
			}
		}
		return list;
	}

	public List<int> GetSubQuestRewardList()
	{
		List<int> list = new List<int>();
		foreach (Tuple<Type, int> item in _listClearIndex)
		{
			if (item.Item1 == typeof(SubQuestData))
			{
				list.Add(item.Item2);
			}
		}
		return list;
	}

	public Vector2 GetAwakenNodePosition()
	{
		if (_awakenRewardNodeLeastPosition != Vector2.zero)
		{
			return _awakenRewardNodeLeastPosition;
		}
		return _awakenNodeLeastPosition;
	}
}
