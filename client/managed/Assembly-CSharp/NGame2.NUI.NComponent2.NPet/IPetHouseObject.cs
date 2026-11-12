using NGame2.NRender;
using NGame2.NUI.NPet;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public interface IPetHouseObject
{
	EPetHouseObjectState EPetHouseObjectState { get; set; }

	EPetHouseObjectType EPetHouseObjectType { get; }

	RenderFacade RenderFacade { get; }

	Vector3 PetHouseObjectPosition { get; }

	bool IsEnable { get; }

	void Close();

	void Update();

	bool CheckHit(RaycastHit hit);

	void Touch();

	void Enable(bool enable);

	void RefreshNotice();
}
