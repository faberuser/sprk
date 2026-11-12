using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PortalDataContainer : JitDataContainerDefault<ChapterDungeonKey2, PortalData>
{
	public const string TABLENAME = "PortalTable";

	private JitDictionary<ChapterDungeonKey2, List<PortalData>> dicChildPortalData = new JitDictionary<ChapterDungeonKey2, List<PortalData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PortalTable");

	public PortalDataContainer()
	{
		base.Packer.RegisterAndBind(dicChildPortalData);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PortalData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(item.ParentNode))
			{
				___Shared_Jit_JitDictionary___.JsonLoad_AddToListValue(key: new ChapterDungeonKey2(item.ParentNode[0], item.ParentNode[1]), dic: dicChildPortalData, value: item);
			}
		}
	}

	public PortalData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public PortalData GetData(string name)
	{
		foreach (PortalData allDatum in GetAllData())
		{
			if (allDatum.Name == name)
			{
				return allDatum;
			}
		}
		return null;
	}

	public IList<PortalData> GetChildDatas(PortalData parentData)
	{
		if (!parentData.HasChild)
		{
			return null;
		}
		return ___Shared_Jit_JitDictionary___.GetOrDefault(key: new ChapterDungeonKey2(parentData.ChapterIndex, parentData.DungeonIndex), This: dicChildPortalData);
	}

	public PortalData CheckPortalOpenEffectContidion(int chapterIndex, int dungeonIndex)
	{
		foreach (PortalData allDatum in GetAllData())
		{
			if (allDatum.Condition != null && allDatum.Condition.Length >= 3)
			{
				int valueInt = ArgParser.GetValueInt(allDatum.Condition, 1);
				int valueInt2 = ArgParser.GetValueInt(allDatum.Condition, 2);
				if (valueInt == chapterIndex && valueInt2 == dungeonIndex)
				{
					return allDatum;
				}
			}
		}
		return null;
	}
}
