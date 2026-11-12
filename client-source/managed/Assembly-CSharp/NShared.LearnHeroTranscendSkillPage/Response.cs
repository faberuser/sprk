namespace NShared.LearnHeroTranscendSkillPage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroInfo Hero { get; set; }
}
