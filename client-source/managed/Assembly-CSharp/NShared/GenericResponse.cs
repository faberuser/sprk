namespace NShared;

public abstract class GenericResponse<ResultType> : AppliableResponse
{
	public ResultType Result { get; set; }
}
