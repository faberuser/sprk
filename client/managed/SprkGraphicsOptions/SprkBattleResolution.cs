using System;

public sealed class SprkBattleResolution
{
	private const int BaseRaise = 12;

	private const int MaxRaise = 96;

	private const int OscillationWindow = 40;

	private int maximum = 100;

	private int target;

	private int over;

	private int under;

	private int raiseAfter = 12;

	private int sinceRaise = -1;

	public int Current { get; private set; } = 100;

	public int RaiseAfter => raiseAfter;

	public void Configure(int scale, int fps)
	{
		if (maximum != scale || target != fps)
		{
			maximum = scale;
			target = fps;
			Restore();
		}
	}

	public void Restore()
	{
		Current = maximum;
		over = (under = 0);
		raiseAfter = 12;
		sinceRaise = -1;
	}

	public void Submit(double gpu, double cpu)
	{
		if (target == 0 || double.IsNaN(gpu) || double.IsInfinity(gpu) || gpu <= 0.0 || gpu >= 250.0 || double.IsNaN(cpu) || double.IsInfinity(cpu))
		{
			over = (under = 0);
			return;
		}
		double num = 1000.0 / (double)target;
		over = ((gpu > num * 1.1 && gpu >= cpu * 0.9) ? (over + 1) : 0);
		under = ((gpu < num * 0.7) ? (under + 1) : 0);
		if (sinceRaise >= 0 && ++sinceRaise > 40)
		{
			sinceRaise = -1;
			raiseAfter = Math.Max(12, raiseAfter / 2);
		}
		if (over >= 6)
		{
			if (Current <= 75)
			{
				over = 0;
				return;
			}
			if (sinceRaise >= 0)
			{
				raiseAfter = Math.Min(96, raiseAfter * 2);
			}
			Current -= 25;
			over = (under = 0);
			sinceRaise = -1;
		}
		else if (under >= raiseAfter)
		{
			if (Current >= maximum)
			{
				under = 0;
				return;
			}
			Current += 25;
			over = (under = 0);
			sinceRaise = 0;
		}
	}
}
