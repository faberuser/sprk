namespace NShared.GetOnestoreIncompleteAppPurchaseInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerInAppPurchaseInfo[] InAppPurchaseInfos { get; set; }
}
