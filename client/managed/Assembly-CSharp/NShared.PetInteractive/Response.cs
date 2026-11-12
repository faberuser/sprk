namespace NShared.PetInteractive;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetInfo PetResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
