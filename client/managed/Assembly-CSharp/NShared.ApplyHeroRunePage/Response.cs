namespace NShared.ApplyHeroRunePage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroInfo HeroInfo { get; set; }

	public HeroRunePageInfo[] HeroRunePageInfos { get; set; }
}
