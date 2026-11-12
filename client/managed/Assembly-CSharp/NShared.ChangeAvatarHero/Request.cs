namespace NShared.ChangeAvatarHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/change_avatar_hero";

	public int AvatarHeroIndex { get; set; }

	public int HeroIndex { get; set; }
}
