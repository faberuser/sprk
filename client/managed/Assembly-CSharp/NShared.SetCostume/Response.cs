namespace NShared.SetCostume;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroCostumeResultInfo HeroCostumeResult { get; set; }
}
