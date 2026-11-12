using System.Collections.Generic;
using UnityEngine;

namespace NGameAsset.NCreature;

public interface IModelData
{
	int Star { get; }

	string Prefix { get; }

	string GetFacialNodeName(int index);

	GameObject InstantiateModel(GameObject assetGo);

	List<SelfEffectData> GetSelfEffectList();
}
