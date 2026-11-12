using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ManualSubCodeComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickComponent(ManualSubCodeComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/Manual/ManualSubCodeComponent";

	public static PrefabAuto<ManualSubCodeComponent> PrefabAuto = new PrefabAuto<ManualSubCodeComponent>("Prefab2/Component/Manual/ManualSubCodeComponent");

	public void Open(Listener listener)
	{
	}

	public void OnClickComponent()
	{
	}

	public void SetSelected(bool select)
	{
	}
}
