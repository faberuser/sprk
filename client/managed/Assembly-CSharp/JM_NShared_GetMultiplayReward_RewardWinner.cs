using System.Collections;
using MiniJSON;
using NShared;
using NShared.GetMultiplayReward;

public static class JM_NShared_GetMultiplayReward_RewardWinner
{
	public static RewardWinner Parse(string s)
	{
		return Parse(s, new RewardWinner());
	}

	public static RewardWinner Parse(string s, RewardWinner o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RewardWinner Parse(IDictionary d)
	{
		return Parse(d, new RewardWinner());
	}

	public static RewardWinner Parse(IDictionary d, RewardWinner o)
	{
		o.ApplicantIds = BaseJsonMarshaler.ParseArrayCast(d, "ApplicantIds", BaseJsonMarshaler.CastToLong);
		o.DiceNumbers = BaseJsonMarshaler.ParseArrayCast(d, "DiceNumbers", BaseJsonMarshaler.CastToInt);
		o.WinnerId = BaseJsonMarshaler.ParseValueLong(d, "WinnerId");
		return o;
	}
}
