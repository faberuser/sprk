namespace NShared.PetEndExplore;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetInfo[] PetResultInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public PetExploreInfo PetExploreInfo { get; set; }
}
