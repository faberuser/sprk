namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ServerType
{
	none,
	gameserver,
	gmsserver,
	battleserver,
	battlecheckserver,
	battlesimulationserver,
	rankingserver,
	globalrankingserver,
	recommenddeckserver,
	deliveryserver,
	couponserver,
	dbqueryserver,
	eventserver,
	billingserver,
	logserver,
	relayserver,
	analyticsserver,
	globalstatsserver
}
