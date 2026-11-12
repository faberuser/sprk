using NGameAsset.NNgui;
using UnityEngine;

namespace NGameAsset;

public interface INguiPrefab : IAsyncLoadBase
{
	void Close();

	NguiInstance CreateInstance(Transform parentTm, int layer);
}
