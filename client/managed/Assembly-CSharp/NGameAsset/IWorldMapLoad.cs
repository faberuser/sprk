using NGameAsset.NWorldMap;
using UnityEngine;

namespace NGameAsset;

public interface IWorldMapLoad : IAsyncLoadBase
{
	int MapImageCount { get; }

	MapImageData GetImageData(int index);

	Material GetImageAsset(int index);

	void Close();
}
