using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CampaignMultiAvatarDataContainer : JitDataContainerDefault<CampaignMultiAvatarKey, CampaignMultiAvatarData>
{
	public const string TABLENAME = "CampaignMultiAvatarTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignMultiAvatarTable");

	public List<CampaignMultiAvatarData> GetDataList(int chapterIndex)
	{
		IList<CampaignMultiAvatarData> allData = GetAllData();
		List<CampaignMultiAvatarData> list = new List<CampaignMultiAvatarData>();
		list.Clear();
		CampaignMultiAvatarData campaignMultiAvatarData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			campaignMultiAvatarData = allData[i];
			if (campaignMultiAvatarData != null && campaignMultiAvatarData.ChapterIndex == chapterIndex && campaignMultiAvatarData.Opened)
			{
				list.Add(campaignMultiAvatarData);
			}
		}
		return list;
	}

	public CampaignMultiAvatarData GetData(int chapterIndex, int lineIndex)
	{
		List<CampaignMultiAvatarData> dataList = GetDataList(chapterIndex);
		new List<CampaignMultiAvatarData>().Clear();
		CampaignMultiAvatarData campaignMultiAvatarData = null;
		for (int i = 0; i < dataList.Count; i++)
		{
			campaignMultiAvatarData = dataList[i];
			if (campaignMultiAvatarData != null && campaignMultiAvatarData.LineIndex == lineIndex)
			{
				return campaignMultiAvatarData;
			}
		}
		return null;
	}

	public bool IsChapter(int chapterIndex)
	{
		List<CampaignMultiAvatarData> dataList = GetDataList(chapterIndex);
		if (dataList != null)
		{
			return dataList.Count > 0;
		}
		return false;
	}
}
