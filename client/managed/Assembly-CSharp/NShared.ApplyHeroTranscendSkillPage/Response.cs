namespace NShared.ApplyHeroTranscendSkillPage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroInfo Hero { get; set; }
}
