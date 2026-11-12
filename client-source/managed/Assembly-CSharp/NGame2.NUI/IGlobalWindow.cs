using NGame2.NScene.NPreloader;

namespace NGame2.NUI;

public interface IGlobalWindow
{
	void ReserveCache(UiPreloader preloader);

	bool IsOpen();

	void Close();
}
