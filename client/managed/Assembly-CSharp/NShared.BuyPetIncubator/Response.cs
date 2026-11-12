namespace NShared.BuyPetIncubator;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorInfo PetIncubatorInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
