namespace NShared;

public interface ITriggerByHeal : ITriggerAmount
{
	long GetOverHeal();

	HealFlag GetFlag();
}
