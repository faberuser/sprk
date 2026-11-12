namespace NShared.UnsetCostume;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroCostumeResultInfo HeroCostumeResult { get; set; }
}
