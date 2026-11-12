using NGame2.NUI.NPet;

namespace NGame2.NUI.NWindow.NPet;

public interface IPetHouseObjectProvider
{
	PetHouseWindow.PetHouseObjectInfo GetPetHouseObjectInfo(EPetHouseObjectType ePetHouseObjectType);
}
