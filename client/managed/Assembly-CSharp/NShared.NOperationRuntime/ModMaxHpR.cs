namespace NShared.NOperationRuntime;

public class ModMaxHpR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MaxHp };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}

	protected override void OnBegin()
	{
		ModMaxHp.HandleMaxHpChanged(this, delegate
		{
			base.OnBegin();
		});
	}

	protected override void OnStatUpdatedAndMakeDirty()
	{
		ModMaxHp.HandleMaxHpChanged(this, base.MakeCreatureStatDirty);
	}
}
