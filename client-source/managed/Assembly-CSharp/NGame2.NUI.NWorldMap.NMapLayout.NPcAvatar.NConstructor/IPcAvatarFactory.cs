namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;

public interface IPcAvatarFactory
{
	void ClearCache();

	PcAvatarStock CreateAvatar(string[] values);
}
