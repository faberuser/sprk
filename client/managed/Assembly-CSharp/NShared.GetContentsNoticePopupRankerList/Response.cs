namespace NShared.GetContentsNoticePopupRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public ContentsNoticePopupRankerInfo[] RankerInfos { get; set; }

	public ContentsNoticePopupServerTotalRankingInfo[] ServerRankInfos { get; set; }
}
