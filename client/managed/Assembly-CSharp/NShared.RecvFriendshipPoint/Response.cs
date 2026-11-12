namespace NShared.RecvFriendshipPoint;

[Response]
public class Response : GenericResponse<ResultType>
{
	public FriendshipPointResultInfo FriendshipPointResult { get; set; }

	public FriendshipPointInfo[] FriendshipPointInfos { get; set; }
}
