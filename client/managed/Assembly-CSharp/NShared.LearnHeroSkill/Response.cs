namespace NShared.LearnHeroSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }
}
