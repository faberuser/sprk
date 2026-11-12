using UnityEngine;

namespace NGameAsset;

public interface IPlainAssetLoad2 : IAsyncLoadBase
{
	string Path { get; }

	ILockCounter Lock { get; }

	WWW WWW { get; }

	Texture2D Texture { get; }

	string Text { get; }

	void StartLoad();

	void Close();
}
