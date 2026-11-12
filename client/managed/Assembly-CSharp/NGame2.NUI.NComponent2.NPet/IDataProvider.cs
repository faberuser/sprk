using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NComponent2.NPet;

public interface IDataProvider
{
	PetInfo GetSelectedPetInfo();

	List<PetInfo> GetPetInfoList();
}
