namespace NShared.FeedThePet;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetInfo PetResultInfo { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
