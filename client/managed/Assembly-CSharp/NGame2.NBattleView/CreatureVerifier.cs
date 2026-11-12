using NVespa;

namespace NGame2.NBattleView;

public class CreatureVerifier : IVerifier
{
	private int _uid;

	public CreatureVerifier(int uid)
	{
		_uid = uid;
	}

	bool IVerifier.IsValid()
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return false;
		}
		return BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(_uid) != null;
	}
}
