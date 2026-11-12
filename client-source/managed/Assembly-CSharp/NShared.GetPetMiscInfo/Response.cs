namespace NShared.GetPetMiscInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetMiscInfo[] PetMiscInfos { get; set; }
}
