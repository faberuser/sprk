namespace NShared;

[JsonSerializable]
public class GuildBuildingInfo : IAccountAttrKey<int>
{
	public bool ReservedUpdate;

	int IAccountAttrKey<int>.Key => BuildingIndex;

	public int BuildingIndex { get; set; }

	public byte BuildingLevel { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }

	public string UpdatedTime { get; set; }
}
