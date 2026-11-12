using NShared;

namespace NGame2.NMessage.NDirect;

public static class CallbackHashCode
{
	public static class BeginFireSkill
	{
		public static int GetHashCode(int skillIndex)
		{
			return skillIndex;
		}

		public static int GetHashCode(NShared.BeginFireSkill callback)
		{
			return GetHashCode(callback.SkillId);
		}
	}

	public static class FireSkill
	{
		public static int GetHashCode(int skillIndex)
		{
			return skillIndex;
		}

		public static int GetHashCode(NShared.FireSkill callback)
		{
			return GetHashCode(callback.SkillId);
		}
	}
}
