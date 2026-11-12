namespace NShared.StartDispatch;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DispatchBattleInfo DispatchBattleInfo { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
