namespace NShared.EggSupplier;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PetAddResultInfo[] PetGachaItemResults { get; set; }

	public PetMiscInfo[] PetMiscInfos { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }
}
