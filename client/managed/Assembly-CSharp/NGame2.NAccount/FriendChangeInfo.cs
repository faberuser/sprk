namespace NGame2.NAccount;

public class FriendChangeInfo
{
	public enum ChangeType
	{
		None,
		AddInvitor,
		AvatarHeroIndex,
		RemoveFriend,
		NickChange
	}

	public ChangeType Type;

	public long FriendId;
}
