namespace NShared.ExecPetGacha;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public PetAddResultInfo[] PetGachaItemResults { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public PetMiscInfo[] PetMiscInfos { get; set; }

	public StaminaResultInfo[] StaminaResults { get; set; }
}
