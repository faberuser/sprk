namespace NShared.UseCoupon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "promotion/use_coupon";

	public string CouponCode { get; set; }

	public string CountryCode { get; set; }
}
