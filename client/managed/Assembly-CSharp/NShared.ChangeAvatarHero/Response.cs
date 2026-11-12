namespace NShared.ChangeAvatarHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerAvatarHeroInfo PlayerAvatarHeroInfo { get; set; }
}
