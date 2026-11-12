using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroFriendlyActionData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => HeroIndex;

	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public string[] BaseSpeak { get; set; }

	[Key(2)]
	public FriendlyActionType Action1 { get; set; }

	[Key(3)]
	public string[] ActionResponse1 { get; set; }

	[Key(4)]
	public FriendlyActionType Action2 { get; set; }

	[Key(5)]
	public string[] ActionResponse2 { get; set; }

	[Key(6)]
	public FriendlyActionType Action3 { get; set; }

	[Key(7)]
	public string[] ActionResponse3 { get; set; }

	[Key(8)]
	public FriendlyActionType Action4 { get; set; }

	[Key(9)]
	public string[] ActionResponse4 { get; set; }

	[Key(10)]
	public FriendlyActionType Action5 { get; set; }

	[Key(11)]
	public string[] ActionResponse5 { get; set; }

	[Key(12)]
	public FriendlyActionType Action6 { get; set; }

	[Key(13)]
	public string[] ActionResponse6 { get; set; }

	[Key(14)]
	public FriendlyActionType Action1_MAX { get; set; }

	[Key(15)]
	public string[] Response1_MAX { get; set; }

	[Key(16)]
	public FriendlyActionType MaxAction1 { get; set; }

	[Key(17)]
	public string[] MaxResponse1 { get; set; }

	[Key(18)]
	public FriendlyActionType MaxAction2 { get; set; }

	[Key(19)]
	public string[] MaxResponse2 { get; set; }

	[Key(20)]
	public FriendlyActionType MaxAction3 { get; set; }

	[Key(21)]
	public string[] MaxResponse3 { get; set; }

	public override string ToString()
	{
		return "HeroFriendlyActionData HeroIndex=" + HeroIndex;
	}

	public string[] GetResponse(FriendlyActionType actionType)
	{
		if (actionType == Action1)
		{
			return ActionResponse1;
		}
		if (actionType == Action2)
		{
			return ActionResponse2;
		}
		if (actionType == Action3)
		{
			return ActionResponse3;
		}
		if (actionType == Action4)
		{
			return ActionResponse4;
		}
		if (actionType == Action5)
		{
			return ActionResponse5;
		}
		if (actionType == Action6)
		{
			return ActionResponse6;
		}
		if (actionType == Action1_MAX)
		{
			return Response1_MAX;
		}
		if (actionType == MaxAction1)
		{
			return MaxResponse1;
		}
		if (actionType == MaxAction2)
		{
			return MaxResponse2;
		}
		if (actionType == MaxAction3)
		{
			return MaxResponse3;
		}
		return null;
	}
}
