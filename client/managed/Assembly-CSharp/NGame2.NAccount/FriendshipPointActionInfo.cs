namespace NGame2.NAccount;

public class FriendshipPointActionInfo
{
	public enum EChangeType
	{
		DidAction,
		ListUpdated
	}

	public EChangeType ChangeType;

	public long friendId;
}
