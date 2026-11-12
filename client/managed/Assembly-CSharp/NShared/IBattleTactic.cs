namespace NShared;

public interface IBattleTactic : ISnapshotObject
{
	void Init(BattleInstance battleInstance, CampaignTacticData tacticData);

	void Update(long currentGlobalTimeMs);

	void OnEvent(IBattleTacticEvent tacticEvent, long currentGlobalTimeMs);
}
