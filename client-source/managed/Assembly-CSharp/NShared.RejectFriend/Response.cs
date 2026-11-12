namespace NShared.RejectFriend;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int DailyRemoveFriends { get; set; }

	public string DailyRemoveFriendsResetTime { get; set; }
}
