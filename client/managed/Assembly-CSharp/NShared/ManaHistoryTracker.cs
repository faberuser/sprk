namespace NShared;

public class ManaHistoryTracker
{
	private const int TICKTIME_MULTIPLIER = 10;

	public long Total_ManaRestored_In_UnitTime;

	private long[] HistoryBuffer;

	private int RecordUnitTime = 1;

	private int RecordArraySize;

	private int currentMPGenHistoryIndex;

	private bool enable;

	public BattleCreature Owner { get; private set; }

	public void Init(BattleCreature owner)
	{
		Owner = owner;
		if (Owner.BattleInstance != null && Owner.BattleInstance.BattleDefine != null && (Owner.BattleInstance.BattleDefine.BattleMenu & EBattleMenu.MPSDamageMeter) != 0)
		{
			RecordUnitTime = BattleConstant.ManaMeterUnitTime;
			RecordArraySize = RecordUnitTime * 10;
			HistoryBuffer = new long[RecordArraySize];
			enable = true;
		}
	}

	public void Update()
	{
		if (enable && HistoryBuffer != null)
		{
			AddHistory();
			MoveHistoryIndex();
			RemoveOldHistory();
			ClearOldHistory();
		}
	}

	private void AddHistory()
	{
		Total_ManaRestored_In_UnitTime += HistoryBuffer[currentMPGenHistoryIndex];
	}

	private void MoveHistoryIndex()
	{
		currentMPGenHistoryIndex++;
		if (currentMPGenHistoryIndex == RecordArraySize)
		{
			currentMPGenHistoryIndex = 0;
		}
	}

	private void RemoveOldHistory()
	{
		Total_ManaRestored_In_UnitTime -= HistoryBuffer[currentMPGenHistoryIndex];
	}

	private void ClearOldHistory()
	{
		HistoryBuffer[currentMPGenHistoryIndex] = 0L;
	}

	public void Record(long amount)
	{
		if (enable)
		{
			HistoryBuffer[currentMPGenHistoryIndex] += amount;
		}
	}

	public long Get_ManaPerUnitTime()
	{
		return Total_ManaRestored_In_UnitTime / RecordUnitTime;
	}

	public long GetAllRecordSum()
	{
		return Total_ManaRestored_In_UnitTime;
	}
}
