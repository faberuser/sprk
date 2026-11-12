namespace NShared.PunishmentRaid;

[JsonSerializable(Javascript = true)]
public class Request
{
	public int GroupIndex { get; set; }

	public EPunishmentDungeonType DungeonType { get; set; }

	public int FlaskItemIndex { get; set; }

	public int FlaskItemCount { get; set; }

	public int[] AffixIndices { get; set; }

	public int Level { get; set; }
}
