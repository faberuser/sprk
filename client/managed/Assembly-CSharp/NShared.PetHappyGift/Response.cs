namespace NShared.PetHappyGift;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public PetInfo PetResult { get; set; }

	public StaminaResultInfo[] StaminaResults { get; set; }
}
