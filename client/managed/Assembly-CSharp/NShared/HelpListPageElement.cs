namespace NShared;

public class HelpListPageElement
{
	public string ComponentName { get; set; }

	public string TitleKey { get; set; }

	public string DescKey { get; set; }

	public string TexturePath { get; set; }

	public int TextureWidth { get; set; }

	public int TextureHeight { get; set; }

	public bool IsValid => !string.IsNullOrEmpty(ComponentName);

	public HelpListPageElement(string componentName, string titleKey, string descKey, string texturePath, int textureWidth, int textureHeight)
	{
		ComponentName = componentName;
		TitleKey = titleKey;
		DescKey = descKey;
		TexturePath = texturePath;
		TextureWidth = textureWidth;
		TextureHeight = textureHeight;
	}
}
