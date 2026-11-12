using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class CampaignChapterDataContainer : JitDataContainerDefault<int, CampaignChapterData>
{
	public const string TABLENAME = "CampaignChapterTable";

	private JitRef<Dictionary<int, CampaignChapterData>> _mainChapterDic = new JitRef<Dictionary<int, CampaignChapterData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignChapterTable");

	public CampaignChapterDataContainer()
	{
		base.Packer.RegisterAndBind(_mainChapterDic);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CampaignChapterData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.MainField && item.IsOpen && !_mainChapterDic.JsonLoad_GetOrNew().ContainsKey(item.Index))
			{
				_mainChapterDic.JsonLoad_GetOrNew().Add(item.Index, item);
			}
		}
	}

	public Dictionary<int, CampaignChapterData> GetMainChapterDataDic()
	{
		return _mainChapterDic.Eval();
	}

	public CampaignChapterData GetMainChapterDataByIndex(int chapterIndex)
	{
		if (_mainChapterDic.Eval().TryGetValue(chapterIndex, out var value))
		{
			return value;
		}
		return null;
	}

	public List<CampaignChapterData> GetChapterDataByType(ChapterType chapterType)
	{
		return (from v in GetAllData()
			where v.Type == chapterType
			select v).ToList();
	}

	public ChapterType GetChapterTypeByIndex(int index)
	{
		return GetData(index)?.Type ?? ChapterType.None;
	}
}
