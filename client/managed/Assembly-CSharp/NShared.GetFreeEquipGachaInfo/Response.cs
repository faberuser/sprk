namespace NShared.GetFreeEquipGachaInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public FreeEquipGachaInfo[] freeEquipGachaInfos { get; set; }
}
