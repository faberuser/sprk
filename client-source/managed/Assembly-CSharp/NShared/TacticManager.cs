namespace NShared;

public class TacticManager : SnapshotObject
{
	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<string, int>> _valueDictionaryHolder;

	private readonly SnapshotObjectMember<IBattleTactic> _tacticHolder;

	private BattleInstance _battleInstance;

	public IBattleTactic Tactic => _tacticHolder.Get();

	public TacticManager()
	{
		_valueDictionaryHolder = new SnapshotObjectMember<IndexableSnapshotDictionary<string, int>>(this);
		_tacticHolder = new SnapshotObjectMember<IBattleTactic>(this);
	}

	public void Init(BattleInstance battleInstance)
	{
		CampaignTacticData data = GameRoot.Instance.CampaignTacticDataContainer.GetData(battleInstance.ChapterIndex, battleInstance.DungeonIndex, battleInstance.Difficulty);
		if (data != null)
		{
			IBattleTactic battleTactic = TacticFactory.Create(data);
			if (battleTactic != null)
			{
				battleTactic.Init(battleInstance, data);
				_battleInstance = battleInstance;
				_valueDictionaryHolder.Alloc();
				_tacticHolder.Set(battleTactic);
			}
		}
	}

	public void Update(long currentGlobalTimeMs, int deltaTimeMs)
	{
		_tacticHolder.Get()?.Update(currentGlobalTimeMs);
	}

	public void SendEvent(IBattleTacticEvent tacticEvent)
	{
		IBattleTactic battleTactic = _tacticHolder.Get();
		if (battleTactic != null)
		{
			long currentTimeMs = _battleInstance.GlobalTimer.GetCurrentTimeMs();
			battleTactic.OnEvent(tacticEvent, currentTimeMs);
		}
	}
}
