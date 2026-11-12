namespace NShared.HeroLimitBreakLevelUp;

[Response]
public class Response : GenericResponse<ResultType>
{
	public sbyte NewHeroLimitBreakLevel { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
