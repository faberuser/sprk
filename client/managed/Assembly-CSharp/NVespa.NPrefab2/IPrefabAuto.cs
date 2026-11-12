namespace NVespa.NPrefab2;

public interface IPrefabAuto
{
	string PrefabPath { get; }

	bool IsGlobal { get; }

	bool IsPooling { get; }

	bool HasPrefab { get; }

	bool IsOpen { get; }

	void OnSceneChanged();
}
