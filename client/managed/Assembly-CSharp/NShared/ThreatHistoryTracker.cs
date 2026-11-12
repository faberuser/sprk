namespace NShared;

public class ThreatHistoryTracker
{
	private const int TICKTIME_MULTIPLIER = 10;

	public long Total_Threat_In_UnitTime;

	private long[] historyBuffer;

	private int recordUnitTime = 1;

	private int recordArraySize;

	private int recoredIndex;

	private int compressRatio = 1;

	private bool enable;

	public BattleCreature Owner { get; private set; }

	public void Init(BattleCreature owner)
	{
		Owner = owner;
		if (Owner.BattleInstance != null && Owner.BattleInstance.BattleDefine != null && (Owner.BattleInstance.BattleDefine.BattleMenu & EBattleMenu.TPSDamageMeter) != 0)
		{
			recordUnitTime = BattleConstant.ThreatMeterUnitTime;
			recordArraySize = recordUnitTime * 10;
			historyBuffer = new long[recordArraySize];
			compressRatio = BattleConstant.ThreatMeterCompressRatio;
			enable = true;
		}
	}

	public void Update()
	{
		if (enable && historyBuffer != null)
		{
			Owner.StatController.IsDead();
			AddHistory();
			MoveHistoryIndex();
			RemoveOldHistory();
			ClearOldHistory();
		}
	}

	private void AddHistory()
	{
		Total_Threat_In_UnitTime += historyBuffer[recoredIndex];
	}

	private void MoveHistoryIndex()
	{
		recoredIndex++;
		if (recoredIndex == recordArraySize)
		{
			recoredIndex = 0;
		}
	}

	private void RemoveOldHistory()
	{
		Total_Threat_In_UnitTime -= historyBuffer[recoredIndex];
	}

	private void ClearOldHistory()
	{
		historyBuffer[recoredIndex] = 0L;
	}

	public void Record(long amount)
	{
		if (enable)
		{
			historyBuffer[recoredIndex] += amount;
		}
	}

	public long Get_ThreatPerUnitTime()
	{
		return Total_Threat_In_UnitTime / recordUnitTime / compressRatio;
	}

	public long GetAllRecordSum()
	{
		return Total_Threat_In_UnitTime / compressRatio;
	}
}
