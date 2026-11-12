using NGame2.NRender;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NBuilding;

public interface IBuilding
{
	string ModelName { get; }

	int Index { get; }

	int Level { get; }

	bool ReservedUpgrade { get; }

	bool HaveUpgradeAnimationKey { get; }

	RenderFacade Model { get; }

	Vector3 Position { get; }

	IBuilding Create(WorldMapBuildData buildData, Transform parent, float render3dAngle);

	void Update();

	void Close();

	void SetExtraAlpha(float alpha);

	void UpgradeCancel();

	void UpgradePrepare();

	void Upgrade();

	void UpgradeComplete();

	void CompleteReservedUpgrade();
}
