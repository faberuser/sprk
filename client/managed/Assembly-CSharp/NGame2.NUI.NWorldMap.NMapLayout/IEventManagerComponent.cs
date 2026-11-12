using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public interface IEventManagerComponent
{
	void Open(EventManager eventManager);

	void Setup(WorldMapEventInfo worldMapEventInfo);

	void Remove(WorldMapEventInfo worldMapEventInfo, bool eatEffect);

	void OnNodeChanged(int dungeonIndex);

	void OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd);

	void ChainUpdate();

	void Visit();

	void Close();
}
