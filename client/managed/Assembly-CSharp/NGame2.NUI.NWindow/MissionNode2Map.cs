using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[RequireComponent(typeof(UIWidget))]
public class MissionNode2Map : MonoBehaviour, CommonNode.IListner
{
	public interface IListner
	{
		void OnClick(MissionComponentData data);
	}

	[Header("[MissionNode2Map]")]
	public Vector2 paddingSize;

	public Vector2 offsetSize;

	public Vector2 componentSize;

	public Vector2 subPaddingSize;

	public Vector2 subComponentSize;

	private IListner _listener;

	private List<MissionNodeComponent> _listNodeCmp = new List<MissionNodeComponent>();

	private List<Tuple<Type, int>> _listClearIndex = new List<Tuple<Type, int>>();

	private Vector2 _awakenNodeLeastPosition = Vector2.zero;

	private Vector2 _awakenRewardNodeLeastPosition = Vector2.zero;

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
		Clear();
		List<MissionVisualData> missionVisualDataList = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(mainCategory, subCategory);
		if (missionVisualDataList == null || __Extension_Method_ICollection__.COUNT(missionVisualDataList) == 0)
		{
			return;
		}
		List<MissionComponentData> list = new List<MissionComponentData>();
		foreach (MissionVisualData item3 in missionVisualDataList)
		{
			string questType = item3.QuestType;
			int questIndex = item3.QuestIndex;
			if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
			{
				continue;
			}
			if (questType.CompareTo("Achievement") == 0)
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(questIndex, 1);
				if (data == null)
				{
					VespaDebug.LogError("MissionNode2Map AchievementData is null. " + questIndex);
					continue;
				}
				MissionComponentData item = new MissionComponentData(data, item3);
				list.Add(item);
			}
			else if (questType.CompareTo("SubQuest") == 0)
			{
				SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(questIndex);
				if (subQuestData == null)
				{
					VespaDebug.LogError("MissionNode2Map SubQuestData is null. " + questIndex);
					continue;
				}
				MissionComponentData item2 = new MissionComponentData(subQuestData, item3);
				list.Add(item2);
			}
		}
		list.Sort(delegate(MissionComponentData a, MissionComponentData b)
		{
			if (a.VisualData != null && b.VisualData != null)
			{
				if (a.VisualData.MainStep > b.VisualData.MainStep)
				{
					return 1;
				}
				if (a.VisualData.MainStep < b.VisualData.MainStep)
				{
					return -1;
				}
				if (a.VisualData.SubStep > b.VisualData.SubStep)
				{
					return 1;
				}
				if (a.VisualData.SubStep < b.VisualData.SubStep)
				{
					return -1;
				}
				return 0;
			}
			return 0;
		});
		int num = 0;
		int num2 = 0;
		foreach (MissionComponentData item4 in list)
		{
			int subStep = item4.VisualData.SubStep;
			if (subStep == 0)
			{
				if (num2 == 0)
				{
					num++;
				}
				else
				{
					num += num2;
					num2 = 0;
				}
				float x = offsetSize.x + (float)(num - 1) * componentSize.x + (float)(num - 1) * paddingSize.x;
				float y = offsetSize.y;
				Vector2 position = new Vector2(x, y);
				CreateNodeComponent(item4, position);
			}
			else if (subStep >= 1)
			{
				float x2 = offsetSize.x + (float)(num - 1) * componentSize.x + (float)(num - 1) * paddingSize.x + (float)(subStep - 1) * subComponentSize.x + (float)(subStep - 1) * subPaddingSize.x;
				float y2 = offsetSize.y - subComponentSize.y - subPaddingSize.y;
				Vector2 position2 = new Vector2(x2, y2);
				CreateNodeComponent(item4, position2, isSubNode: true);
				num2 = subStep;
			}
			else
			{
				VespaDebug.LogError(string.Format("MissionVisualData SubStep is Wrong. " + subStep));
			}
		}
	}

	private void CreateNodeComponent(MissionComponentData data, Vector2 position, bool isSubNode = false)
	{
		if (data == null && data.VisualData == null)
		{
			return;
		}
		MissionNodeComponent missionNodeComponent = null;
		missionNodeComponent = (isSubNode ? MissionNodeComponent.PrefabAuto3.Factory.Create(base.transform, base.gameObject.layer) : MissionNodeComponent.PrefabAuto2.Factory.Create(base.transform, base.gameObject.layer));
		if (missionNodeComponent == null)
		{
			return;
		}
		missionNodeComponent.transform.localScale = Vector3.one;
		missionNodeComponent.Open(data, this, data.VisualData.MainStep, data.VisualData.SubStep, position, Vector2.one, isSubNode ? MissionNodeComponent.ENodeType.SubCard : MissionNodeComponent.ENodeType.MainCard);
		if (isSubNode)
		{
			if (__Extension_Method_ICollection__.COUNT(_listNodeCmp) > 0)
			{
				MissionNodeComponent last = Vespa_Generic_IList_Extension_Method.GetLast(_listNodeCmp);
				missionNodeComponent.DrawLinkedLine(last.GetPosition());
			}
		}
		else if (__Extension_Method_ICollection__.COUNT(_listNodeCmp) > 0)
		{
			for (int num = __Extension_Method_ICollection__.COUNT(_listNodeCmp) - 1; num >= 0; num--)
			{
				MissionNodeComponent missionNodeComponent2 = _listNodeCmp[num];
				if (missionNodeComponent2.ElementData == null || missionNodeComponent2.ElementData.VisualData == null || data.VisualData == null)
				{
					VespaDebug.LogError("MissionNode2Map not found MissionVisualData.");
					break;
				}
				if (missionNodeComponent2.ElementData.VisualData.SubStep == 0 && missionNodeComponent2.ElementData.VisualData.MainStep != data.VisualData.MainStep)
				{
					missionNodeComponent.DrawLinkedLine(missionNodeComponent2.GetPosition());
					break;
				}
			}
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
		_listNodeCmp.Add(missionNodeComponent);
	}

	public void Refresh()
	{
		foreach (MissionNodeComponent item in _listNodeCmp)
		{
			item.Refresh();
		}
		_listClearIndex.Clear();
		foreach (MissionNodeComponent item2 in _listNodeCmp)
		{
			if (item2.IsRewardAble())
			{
				MissionComponentData elementData = item2.ElementData;
				if (elementData != null)
				{
					_listClearIndex.Add(new Tuple<Type, int>(elementData.DataType, elementData.Index));
				}
			}
		}
	}

	private void Clear()
	{
		_awakenNodeLeastPosition = Vector2.zero;
		_awakenRewardNodeLeastPosition = Vector2.zero;
		if (_listNodeCmp != null && __Extension_Method_ICollection__.COUNT(_listNodeCmp) > 0)
		{
			foreach (MissionNodeComponent item in _listNodeCmp)
			{
				item.Clear();
			}
			FactoryBase.ReleaseAndClearList(_listNodeCmp);
		}
		_listNodeCmp.Clear();
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
