namespace NShared.GetPetHouseInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorSlotInfo[] PetIncubatorSlotInfos { get; set; }

	public PetIncubatorInfo[] PetIncubatorInfos { get; set; }

	public PetMiscInfo[] PetMiscInfos { get; set; }
}
