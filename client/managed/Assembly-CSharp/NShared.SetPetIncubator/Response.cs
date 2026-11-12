namespace NShared.SetPetIncubator;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetIncubatorSlotInfo PetIncubatorSlotInfo { get; set; }
}
