namespace NShared.HeroLimitBreakExpUp;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int NewHeroLevel { get; set; }

	public long NewHeroLimitBreakExp { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
