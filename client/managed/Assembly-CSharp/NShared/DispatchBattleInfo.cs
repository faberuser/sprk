namespace NShared;

[JsonSerializable(Javascript = true)]
public class DispatchBattleInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SlotIndex;

	public virtual int SlotIndex { get; set; }

	public virtual int ChapterIndex { get; set; }

	public virtual int DungeonIndex { get; set; }

	public virtual int Difficulty { get; set; }

	public virtual int DeckIndex { get; set; }

	public virtual int RepeatCount { get; set; }

	public virtual string HeroIndices { get; set; }

	public virtual string BeginTime { get; set; }

	public virtual string CompleteTime { get; set; }

	public virtual string ClientResult { get; set; }

	public virtual int ClientResultTimeMs { get; set; }

	public virtual string ServerResult { get; set; }

	public virtual DispatchState State { get; set; }

	public virtual int WinCount { get; set; }

	public virtual int LoseCount { get; set; }

	public virtual int StaminaDiscountRate { get; set; }
}
