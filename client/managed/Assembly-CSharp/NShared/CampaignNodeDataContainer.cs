using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CampaignNodeDataContainer : JitDataContainerDefault<ChapterDungeonKey2, CampaignNodeData>
{
	public const string TABLENAME = "CampaignNodeTable";

	private List<CampaignNodeData> _emptyList = new List<CampaignNodeData>();

	private JitDictionary<ChapterDungeonKey2, HashSet<int>> _prevDungeonDictionary = new JitDictionary<ChapterDungeonKey2, HashSet<int>>();

	private JitDictionary<string, ChapterDungeonKey2> _chapterDungeon_by_EntryNPCCode = new JitDictionary<string, ChapterDungeonKey2>();

	private JitDictionary<CampaignNodeData.EMenuOpenCondition, List<CampaignNodeData>> _lists_by_MenuOpenCondition = new JitDictionary<CampaignNodeData.EMenuOpenCondition, List<CampaignNodeData>>();

	private JitDictionary<ContentsStatusType, List<CampaignNodeData>> _lists_by_ContentsType = new JitDictionary<ContentsStatusType, List<CampaignNodeData>>();

	private JitDictionary<NodeFunctionType, List<CampaignNodeData>> _lists_by_Function = new JitDictionary<NodeFunctionType, List<CampaignNodeData>>();

	private JitDictionary<int, List<CampaignNodeData>> _sortedLists_by_ChapterIndex = new JitDictionary<int, List<CampaignNodeData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignNodeTable");

	public CampaignNodeDataContainer()
	{
		base.Packer.RegisterAndBind(_prevDungeonDictionary);
		base.Packer.RegisterAndBind(_chapterDungeon_by_EntryNPCCode);
		base.Packer.RegisterAndBind(_lists_by_MenuOpenCondition);
		base.Packer.RegisterAndBind(_lists_by_ContentsType);
		base.Packer.RegisterAndBind(_lists_by_Function);
		base.Packer.RegisterAndBind(_sortedLists_by_ChapterIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (CampaignNodeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!String_Extension_Method.IsNullOrEmpty(item.EntryNPCCode) && !_chapterDungeon_by_EntryNPCCode.JsonLoad_Get().ContainsKey(item.EntryNPCCode))
			{
				_chapterDungeon_by_EntryNPCCode.JsonLoad_Get().Add(item.EntryNPCCode, item.GETKEY());
			}
		}
		foreach (CampaignNodeData item2 in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item2.NextNodeIndex != null)
			{
				int chapterIndex = item2.ChapterIndex;
				int dungeonIndex = item2.DungeonIndex;
				for (int i = 0; i < item2.NextNodeIndex.Length; i++)
				{
					int dungeonIndex2 = item2.NextNodeIndex[i];
					___Shared_Jit_JitDictionary___.JsonLoad_AddToHashSetValue(key: new ChapterDungeonKey2(chapterIndex, dungeonIndex2), dic: _prevDungeonDictionary, value: dungeonIndex);
				}
			}
		}
		foreach (CampaignNodeData item3 in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_MenuOpenCondition.JsonLoad_AddToListValue(item3.MenuOpenCondition, item3);
			_lists_by_ContentsType.JsonLoad_AddToListValue(item3.ContentsType, item3);
			_lists_by_Function.JsonLoad_AddToListValue(item3.Function, item3);
			_sortedLists_by_ChapterIndex.JsonLoad_AddToListValue(item3.ChapterIndex, item3);
		}
		_sortedLists_by_ChapterIndex.JsonLoad_SortListValue((CampaignNodeData lhs, CampaignNodeData rhs) => lhs.DungeonIndex.CompareTo(rhs.DungeonIndex));
	}

	public CampaignNodeData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public IList<CampaignNodeData> GetData_by_MenuOpenCondition(CampaignNodeData.EMenuOpenCondition menuOpenCondition)
	{
		return _lists_by_MenuOpenCondition.GetOrDefault(menuOpenCondition, _emptyList);
	}

	public IList<CampaignNodeData> GetData_by_ContentsType(ContentsStatusType contentsType)
	{
		return _lists_by_ContentsType.GetOrDefault(contentsType, _emptyList);
	}

	public IList<CampaignNodeData> GetData_by_Function(NodeFunctionType function)
	{
		return _lists_by_Function.GetOrDefault(function, _emptyList);
	}

	public IList<CampaignNodeData> GetAllChapterData(int chapterIndex)
	{
		return _sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex, _emptyList);
	}

	public HashSet<int> GetPrevDungeonIndices(int chapterIndex, int dungeonIndex)
	{
		return ___Shared_Jit_JitDictionary___.GetOrDefault(key: new ChapterDungeonKey2(chapterIndex, dungeonIndex), This: _prevDungeonDictionary);
	}

	public List<CampaignNodeData> GetAllDataOfFunctionType(NodeFunctionType functionType, int chapterIndex = 0)
	{
		List<CampaignNodeData> list = new List<CampaignNodeData>();
		foreach (CampaignNodeData item in GetData_by_Function(functionType))
		{
			if (item.Function == functionType && (chapterIndex == 0 || chapterIndex == item.ChapterIndex))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<CampaignNodeData> GetAllDataOfContntType(ContentsStatusType type, int chapterIndex = 0)
	{
		List<CampaignNodeData> list = new List<CampaignNodeData>();
		foreach (CampaignNodeData item in GetData_by_ContentsType(type))
		{
			if (item.ContentsType == type && (chapterIndex == 0 || chapterIndex == item.ChapterIndex))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<CampaignNodeData> GetAllPortalDungeonData(int chapterIndex)
	{
		List<CampaignNodeData> list = new List<CampaignNodeData>();
		foreach (CampaignNodeData item in _sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex, _emptyList))
		{
			if (chapterIndex == item.ChapterIndex && (item.PortalPositionX > 0 || item.PortalPositionY > 0))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public string GetNodeName(string npcCode)
	{
		if (!_chapterDungeon_by_EntryNPCCode.TryGetValue(npcCode, out var value))
		{
			return string.Empty;
		}
		CampaignNodeData data = GetData(value);
		if (data == null)
		{
			return string.Empty;
		}
		return data.Name;
	}

	public CampaignNodeData GetEntryMenuNode(string npcCode)
	{
		if (!_chapterDungeon_by_EntryNPCCode.TryGetValue(npcCode, out var value))
		{
			return null;
		}
		return GetData(value);
	}
}
