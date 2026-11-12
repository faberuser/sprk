namespace NShared.GetUserInfoRegion;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PtsEntryCondition PtsEntryCondition { get; set; }

	public PtsUserInfo[] UserInfo { get; set; }
}
