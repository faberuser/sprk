namespace NGameAsset;

public static class DefineCreature
{
	public enum EMotion
	{
		Idle,
		BattleIdle,
		Run,
		Die,
		Damage,
		Stun,
		Down,
		Getup,
		Looting,
		NamedAction,
		EmotiMotion,
		HouseMotion,
		Signature
	}

	public enum EMotionEnd
	{
		End,
		Loop,
		Infinite
	}

	public static class VersionHistory
	{
		public const string Version_20150524 = "20150524";
	}

	public const float DefaultRunSpeed = 5f;

	public const string BundleTypeName = "[Creature]";

	public const string VersionCode = "20150524";

	public static float DefaultBlendTime(EMotion motionType)
	{
		return motionType switch
		{
			EMotion.Idle => 0.3f, 
			EMotion.BattleIdle => 0.3f, 
			EMotion.Run => 0.2f, 
			EMotion.Stun => 0.2f, 
			_ => 0.1f, 
		};
	}

	public static string BundleCode(string code)
	{
		return "Creature." + (String_Extension_Method.IsNullOrEmpty(code) ? "(noname)" : code);
	}

	public static string ModelName(int star, string prefix)
	{
		string text = "Model";
		if (!String_Extension_Method.IsNullOrEmpty(prefix))
		{
			text = text + "_" + prefix;
		}
		if (star > 0)
		{
			text = text + "_" + $"Star{star}";
		}
		return text;
	}

	public static bool NeedLoadAnimation(CreatureStock.ELoadLevel loadLevel, EMotion motionType)
	{
		switch (loadLevel)
		{
		case CreatureStock.ELoadLevel.Minimum:
			if (motionType != EMotion.Idle && motionType != EMotion.BattleIdle && motionType != EMotion.Run)
			{
				return motionType == EMotion.NamedAction;
			}
			return true;
		case CreatureStock.ELoadLevel.PcAvatar:
			if (motionType != EMotion.Idle && motionType != EMotion.BattleIdle && motionType != EMotion.Run)
			{
				return motionType == EMotion.NamedAction;
			}
			return true;
		case CreatureStock.ELoadLevel.Normal:
			return motionType != EMotion.EmotiMotion;
		case CreatureStock.ELoadLevel.IncludeFacial:
			return true;
		default:
			_ = 3;
			return true;
		}
	}
}
