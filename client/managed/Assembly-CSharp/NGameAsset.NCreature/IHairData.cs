using UnityEngine;

namespace NGameAsset.NCreature;

public interface IHairData
{
	string HairName { get; }

	GameObject InstantiateModel(GameObject assetGo);
}
