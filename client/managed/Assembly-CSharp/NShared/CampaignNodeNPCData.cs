using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignNodeNPCData : IDataKeySelector<ChapterDungeonKey2>
{
	public enum NPCAvatarState
	{
		None,
		Appear,
		Alive,
		Dead,
		Lose
	}

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public string[] AppearMotionValues { get; set; }

	[Key(3)]
	public string[] AliveMotionValues { get; set; }

	[Key(4)]
	public string[] DeadMotionValues { get; set; }

	[Key(5)]
	public string[] LoseMotionValues { get; set; }

	public string[] GetMotionValue(NPCAvatarState state)
	{
		if (state == NPCAvatarState.Appear)
		{
			return AppearMotionValues;
		}
		return state switch
		{
			NPCAvatarState.Appear => AliveMotionValues, 
			NPCAvatarState.Dead => DeadMotionValues, 
			NPCAvatarState.Lose => LoseMotionValues, 
			_ => AliveMotionValues, 
		};
	}
}
