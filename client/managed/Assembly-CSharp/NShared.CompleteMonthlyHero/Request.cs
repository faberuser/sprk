namespace NShared.CompleteMonthlyHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "monthly_hero/complete_monthly_hero";

	public int MonthlyHeroIndex { get; set; }
}
