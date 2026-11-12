namespace NShared.GetWorldBossHp;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long Hp { get; set; }
}
