using NShared;

namespace NGame2.NUI.NPet;

public interface IPet
{
	PetInfo PetInfo { get; }

	int PetIndex { get; }

	void Update(bool isTouch, bool notTouchCamera, bool isSleep);

	void Close();

	void Enable(bool isEnable);

	void SetPetInfo(PetInfo petInfo);
}
