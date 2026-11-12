namespace NShared.GetPlayerCurrencyInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerCurrencyInfo PlayerCurrencyInfo { get; set; }
}
