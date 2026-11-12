namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class SetUserAvatarHero
{
	public long AccountId { get; set; }

	public int AvatarHeroIndex { get; set; }
}
