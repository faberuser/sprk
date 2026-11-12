using System;

namespace NGame2.NUI.NPet;

public interface IPetHouseObjectManager
{
	void AlwaysUpdate();

	void Update(UICamera.MouseOrTouch touch);

	void Close();

	void SetStateIdleAllPetHouseObject();

	void SetIncubatorState(int slotIndex, PetIncubatorObjectInfo.IncubatorStateType incubatorStateType, Action onEnd, bool needInfoUpdate);

	void EnableNameComponentList(bool isEnable);

	void FocusPetHouseObject(EPetHouseObjectType ePetHouseObjectType, Action onEnd);

	void AddNewIncubatorObject(int slotIndex);

	void ChangeIncubatorObject(int slotIndex);
}
