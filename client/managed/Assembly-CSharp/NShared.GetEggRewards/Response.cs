namespace NShared.GetEggRewards;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorSlotInfo PetIncubatorSlotInfo { get; set; }

	public PetAddResultInfo[] PetAddResultInfos { get; set; }
}
