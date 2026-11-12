namespace NVespa.NNGUIExtension.NAnimation;

public interface IUpdater
{
	BaseData.EUpdateValueType UpdateValueType { get; }

	void OnUpdate(float delta);

	bool IsEnd();

	void SetEnd();

	void Release();

	void Reset();

	void Refresh();
}
