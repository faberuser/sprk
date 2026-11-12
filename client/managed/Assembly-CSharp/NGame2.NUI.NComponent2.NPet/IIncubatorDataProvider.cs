using NShared;

namespace NGame2.NUI.NComponent2.NPet;

public interface IIncubatorDataProvider
{
	int GetSelectedEggIndex();

	int GetSelectedSlotIndex();

	PetIncubatorSlotInfo GetSelectedSlotInfo();

	EIncubatingState GetIncubatingState();
}
