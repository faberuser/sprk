namespace NShared.GetTowerHeroInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ResultCreatureInfo[] TowerHeroInfos { get; set; }
}
