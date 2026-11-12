using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public interface IPetHouseProvider
{
	FloatRange MoveXRange { get; }

	FloatRange MoveZRange { get; }

	Vector3 SlidePetStartPosition { get; }

	bool IsShowPetHouseUI { get; }

	int PetPlaceCount { get; }

	void CurrentPetsState();
}
