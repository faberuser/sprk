namespace NGame2.NBattleView.NCreature;

public interface IHandler
{
	void Begin();

	bool IsEnd();

	void Cancel();

	void ChainUpdate();
}
