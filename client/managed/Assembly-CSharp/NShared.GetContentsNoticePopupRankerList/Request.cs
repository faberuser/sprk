namespace NShared.GetContentsNoticePopupRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "records_of_honor/get_contents_notice_popup_ranker_list";

	public ContentsNoticeType ContentsType { get; set; }

	public int SeasonIndex { get; set; }
}
