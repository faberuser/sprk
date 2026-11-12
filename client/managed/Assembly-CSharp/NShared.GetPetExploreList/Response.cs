namespace NShared.GetPetExploreList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetExploreInfo[] PetExploreInfos { get; set; }
}
