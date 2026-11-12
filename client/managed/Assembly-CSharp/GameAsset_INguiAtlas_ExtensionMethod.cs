using NGameAsset;

public static class GameAsset_INguiAtlas_ExtensionMethod
{
	public static int GetAtlasWidth(this INguiAtlas This)
	{
		if (This == null)
		{
			return 0;
		}
		if (!This.ISVALID())
		{
			return 0;
		}
		if (This.Value.texture == null)
		{
			return 0;
		}
		if (This.Value.atlasWidth > 0)
		{
			return This.Value.texture.width;
		}
		if (This.Data.Width > 0)
		{
			return This.Data.Width;
		}
		return This.Value.texture.width;
	}

	public static int GetAtlasHeight(this INguiAtlas This)
	{
		if (This == null)
		{
			return 0;
		}
		if (!This.ISVALID())
		{
			return 0;
		}
		if (This.Value.texture == null)
		{
			return 0;
		}
		if (This.Value.atlasHeight > 0)
		{
			return This.Value.texture.height;
		}
		if (This.Data.Height > 0)
		{
			return This.Data.Height;
		}
		return This.Value.texture.height;
	}
}
