namespace NShared.GetDispatchList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DispatchBattleInfo[] DispatchBattleInfos { get; set; }
}
