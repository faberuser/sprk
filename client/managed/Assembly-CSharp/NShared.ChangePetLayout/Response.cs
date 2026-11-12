namespace NShared.ChangePetLayout;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetMiscInfo[] PetMiscInfos { get; set; }
}
