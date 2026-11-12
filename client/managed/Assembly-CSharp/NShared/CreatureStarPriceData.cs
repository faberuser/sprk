using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureStarPriceData : IDataKeySelector<CreatureStarPriceKey>
{
	[Key(0)]
	public CreatureOpenStatusType OpenStatus { get; set; }

	[Key(1)]
	public int Star { get; set; }

	[Key(2)]
	public int BuyGem { get; set; }

	[Key(3)]
	public int Mileage { get; set; }

	CreatureStarPriceKey IDataKeySelector<CreatureStarPriceKey>.Key => new CreatureStarPriceKey(OpenStatus, Star);

	public override string ToString()
	{
		return "OpenStatus=" + OpenStatus.ToString() + " Star=" + Star;
	}
}
