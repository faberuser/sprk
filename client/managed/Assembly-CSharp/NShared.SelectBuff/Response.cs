namespace NShared.SelectBuff;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] SelectedBuffIndices { get; set; }
}
