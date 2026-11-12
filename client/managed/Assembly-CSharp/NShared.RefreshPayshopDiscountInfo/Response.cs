namespace NShared.RefreshPayshopDiscountInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PayShopDiscountInfo[] PayShopDiscountInfos { get; set; }

	public int RefreshTimeSec { get; set; }
}
