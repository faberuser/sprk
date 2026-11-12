using NShared.NJit;

namespace NShared;

public class ContentsNoticePopupDataContainer : JitDataContainerDefault<int, ContentsNoticePopupData>
{
	public const string TABLENAME = "ContentsNoticePopupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ContentsNoticePopupTable");

	public ContentsNoticePopupData GetData(ContentsNoticePopupInfo info, ContentsNoticePopupType popupType)
	{
		ContentsNoticePopupData result = null;
		foreach (ContentsNoticePopupData allDatum in GetAllData())
		{
			if (allDatum.PopupType == popupType && allDatum.ContentsType == info.ContentsType && allDatum.ChapterIndex == info.ChapterIndex && allDatum.DungeonIndex == info.DungeonIndex)
			{
				result = allDatum;
				break;
			}
		}
		return result;
	}
}
