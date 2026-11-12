namespace NGame2.NUI.NWorldMap.NMapLayout;

public interface IObserver
{
	void OnEvent(FocusEvent changedInfo);

	void OnTouch(bool isPress);
}
