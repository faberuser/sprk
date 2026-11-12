namespace NShared.RecruitHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroAddResultInfo HeroResult { get; set; }
}
