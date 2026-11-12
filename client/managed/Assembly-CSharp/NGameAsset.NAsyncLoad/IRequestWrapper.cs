using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public interface IRequestWrapper : IAsyncLoadBase
{
	Object asset { get; }

	void Close();
}
