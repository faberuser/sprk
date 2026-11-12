using System;

namespace NShared;

public abstract class BaseBattleTactic : SnapshotObject, IBattleTactic, ISnapshotObject
{
	protected readonly SnapshotObjectMember<CampaignTacticData> _tacticDataHolder;

	protected BattleInstance _battleInstance;

	public CampaignTacticData TacticData => _tacticDataHolder.Get();

	public BaseBattleTactic()
	{
		_tacticDataHolder = new SnapshotObjectMember<CampaignTacticData>(this, null, GameRoot.Instance.CampaignTacticDataContainer);
	}

	void IBattleTactic.Init(BattleInstance battleInstance, CampaignTacticData tacticData)
	{
		_battleInstance = battleInstance;
		_tacticDataHolder.Set(tacticData);
		OnInit();
	}

	void IBattleTactic.Update(long currentGlobalTimeMs)
	{
		OnUpdate(currentGlobalTimeMs);
	}

	void IBattleTactic.OnEvent(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs)
	{
		OnEvent(tacticEvent, currentGlobalTimeMs);
	}

	public virtual void OnInit()
	{
	}

	public virtual void OnUpdate(long currentGlobalTimeMs)
	{
	}

	public abstract void OnEvent(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs);

	protected static void HandleEvent<T>(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs, Action<T, long> action) where T : class, IBattleTacticEvent
	{
		if (action != null && tacticEvent is T arg)
		{
			action(arg, currentGlobalTimeMs);
		}
	}
}
