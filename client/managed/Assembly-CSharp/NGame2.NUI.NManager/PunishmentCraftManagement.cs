using NGame2.NUI.NWindow;

namespace NGame2.NUI.NManager;

public class PunishmentCraftManagement : BaseManagement<PunishmentCraftManagement>
{
	protected override void Awake()
	{
		AddInstance<PunishmentRuneCraftWindow>("Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftWindow");
	}

	public void OpenPunishmentRuneCraftWindow()
	{
		PunishmentRuneCraftWindow punishmentRuneCraftWindow = GetInstance<PunishmentRuneCraftWindow>();
		if (!punishmentRuneCraftWindow.IsOpen())
		{
			punishmentRuneCraftWindow.Open();
		}
	}
}
