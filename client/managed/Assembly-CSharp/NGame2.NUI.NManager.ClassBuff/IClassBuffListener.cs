using NShared;

namespace NGame2.NUI.NManager.ClassBuff;

public interface IClassBuffListener
{
	void AddPoint();

	void AddPoint(ItemData itemData);

	void AddPoint(HeroExpResultInfo info);
}
