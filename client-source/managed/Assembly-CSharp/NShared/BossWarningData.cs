using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class BossWarningData : IDataKeySelector<int>
{
	public enum EWarningType
	{
		Text,
		Image
	}

	public enum EType
	{
		Skill,
		Buff
	}

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SkillIndex { get; set; }

	[Key(2)]
	public EType WarningTargetType { get; set; }

	[Key(3)]
	public EWarningType SkillWarningType { get; set; }

	[Key(4)]
	public string SkillWarningOption { get; set; }

	[Key(5)]
	public string BuffStartWarningType { get; set; }

	[Key(6)]
	public string BuffStartWarningOption { get; set; }

	[Key(7)]
	public int BuffStartStackCount { get; set; }

	[Key(8)]
	public string BuffCrashWarningType { get; set; }

	[Key(9)]
	public string BuffCrashWarningOption { get; set; }

	[Key(10)]
	public int BuffCrashStackCount { get; set; }

	[Key(11)]
	public string EffectType { get; set; }

	[Key(12)]
	public float DelayTime { get; set; }

	[Key(13)]
	public float DurationTime { get; set; }

	[Key(14)]
	public int Priority { get; set; }

	[Key(15)]
	public bool CantCancel { get; set; }

	int IDataKeySelector<int>.Key => SkillIndex;
}
