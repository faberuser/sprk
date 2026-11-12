namespace NShared.ChangeRecruitHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerHeroFriendlyInfo FriendlyInfo { get; set; }
}
