namespace NShared.PetGachaRoofReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public PetGachaCountInfo PetGachaCountInfo { get; set; }
}
