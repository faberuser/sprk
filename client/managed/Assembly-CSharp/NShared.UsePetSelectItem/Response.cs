namespace NShared.UsePetSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public ItemUseResultInfo[] ItemUseResults { get; set; }

	public PetAddResultInfo PetAddResultInfo { get; set; }
}
