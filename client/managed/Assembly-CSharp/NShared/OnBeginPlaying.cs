namespace NShared;

public class OnBeginPlaying : BaseSkillTrigger
{
	private readonly SnapshotObjectMember<int> _waveIndexHolder;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnBeginPlaying;

	public int WaveIndex
	{
		get
		{
			return _waveIndexHolder.Get();
		}
		set
		{
			_waveIndexHolder.Set(value);
		}
	}

	public OnBeginPlaying()
	{
		_waveIndexHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		if (valueGetter.GetTriggerValue(firedCreature, 1, out string value) && !string.IsNullOrEmpty(value) && !ArgParser.HasValueInt(value, WaveIndex))
		{
			return false;
		}
		return true;
	}
}
