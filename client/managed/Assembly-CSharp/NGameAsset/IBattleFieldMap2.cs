using NVespa;
using UnityEngine;

namespace NGameAsset;

public interface IBattleFieldMap2
{
	void Close();

	void SetParent(Transform parentTm, Camera cam);

	RenderGroupModifier2 GetRenderGroupModifier();

	void ChainUpdate(ILocalTimer localTimer, Camera cam);

	void OnEvent(EBattleFieldEventType eventType, int eventValue);
}
