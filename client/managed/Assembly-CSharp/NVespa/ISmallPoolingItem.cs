namespace NVespa;

public interface ISmallPoolingItem
{
	void OnCreateNew();

	void OnDestroy();

	void OnCreate();

	void OnRelease();
}
