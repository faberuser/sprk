namespace NShared.SetEggInPetIncubator;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorSlotInfo PetIncubatorSlotInfo { get; set; }

	public ItemResultInfo ItemResult { get; set; }
}
