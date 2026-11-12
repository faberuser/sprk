namespace NShared.PetCancelExplore;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetInfo[] PetResultInfos { get; set; }

	public PetExploreInfo PetExploreInfo { get; set; }
}
