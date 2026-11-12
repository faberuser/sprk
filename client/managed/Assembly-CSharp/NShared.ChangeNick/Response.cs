using NShared.QueryNick;

namespace NShared.ChangeNick;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string CreatedTime { get; set; }
}
