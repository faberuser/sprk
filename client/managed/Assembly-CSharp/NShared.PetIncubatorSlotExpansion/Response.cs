namespace NShared.PetIncubatorSlotExpansion;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorSlotInfo[] PetIncubatorSlotInfos { get; set; }

	public PetIncubatorInfo[] PetIncubatorInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
