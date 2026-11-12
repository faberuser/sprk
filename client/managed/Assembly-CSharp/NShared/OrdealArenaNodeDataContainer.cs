using System;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class OrdealArenaNodeDataContainer : JitDataContainerDefault<ChapterDungeonKey2, OrdealArenaNodeData>
{
	public const string TABLENAME = "OrdealArenaNodeTable";

	private JitDictionary<int, int> _dicMaxFloor = new JitDictionary<int, int>();

	private JitDictionary<int, OrdealArenaNodeData> _dicLinkedByIndex = new JitDictionary<int, OrdealArenaNodeData>();

	private JitRef<HashSet<int>> _hashChapter = new JitRef<HashSet<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaNodeTable");

	public OrdealArenaNodeDataContainer()
	{
		base.Packer.RegisterAndBind(_dicMaxFloor);
		base.Packer.RegisterAndBind(_dicLinkedByIndex);
		base.Packer.RegisterAndBind(_hashChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.BaseList.JsonLoad_GetOrNew();
		Dictionary<int, int> dictionary = _dicMaxFloor.JsonLoad_Get();
		foreach (OrdealArenaNodeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_hashChapter.JsonLoad_GetOrNew().Add(item.ChapterIndex);
			_dicLinkedByIndex.JsonLoad_AddIfNotExist(item.Index, item);
			if (!dictionary.TryGetValue(item.ChapterIndex, out var value))
			{
				value = 0;
			}
			dictionary[item.ChapterIndex] = Math.Max(value, item.Floor);
		}
	}

	public bool IsOrdealArenaChapter(int chapterIndex)
	{
		return _hashChapter.Eval().Contains(chapterIndex);
	}

	public int GetMaxFloor(int chapterIndex)
	{
		return _dicMaxFloor.GetOrDefault(chapterIndex);
	}

	public OrdealArenaNodeData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public OrdealArenaNodeData GetDataByIndex(int index)
	{
		return _dicLinkedByIndex.GetOrDefault(index);
	}
}
