namespace NGame2.NBattleView.NCreature.NHandler;

public interface ILooting
{
	float CalculateIfAdd(DropItem dropItem);

	void AddDropItem(DropItem dropItem);

	bool IsLootingEnd();
}
