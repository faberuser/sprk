using System.Collections.Generic;
using NGame2.NUI.NWindow.NPet;

namespace NGame2.NUI.NPet;

public interface IPetHouseManager
{
	void Update(UICamera.MouseOrTouch touch);

	void Close();

	void PlacePet(float sec, List<PlacePetInfo> placePetInfoList);

	void AllPetEnable(bool isEnable);

	void NextPetInteraction(int currentPeIndex, int nextIndex);

	void AnnouncePetsCurrentState();
}
