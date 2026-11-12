namespace NShared.BeginEclipse;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string BattleServerAddress { get; set; }

	public int BattleServerPort { get; set; }
}
