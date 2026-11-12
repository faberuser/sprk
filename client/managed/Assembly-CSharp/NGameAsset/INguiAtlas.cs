using NGameAsset.NNguiAtlas;

namespace NGameAsset;

public interface INguiAtlas : IAsyncLoad<UIAtlas>, IAsyncLoadBase
{
	AtlasData Data { get; }

	bool HasAlphaTexture { get; }

	void Close(bool destroyImmediately);
}
