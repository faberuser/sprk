using System.Collections.Generic;
using NGameAsset.NBattleFieldMap2;
using UnityEngine;

namespace NGameAsset;

public interface IBattleFieldMapOpenData
{
	BattleFieldMapStock Stock { get; }

	IModelData Model { get; }

	List<EffectFxData> EffectFxList { get; }

	GameObject InstantiateModel();
}
