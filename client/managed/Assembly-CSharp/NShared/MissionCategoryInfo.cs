namespace NShared;

[JsonSerializable(Javascript = true)]
public class MissionCategoryInfo
{
	public int MainCategoryIndex { get; set; }

	public int SubCategoryIndex { get; set; }
}
