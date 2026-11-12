using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NInventory;
using NShared;
using NVespa.NSingleton;
using NVespa.NTask;

namespace NGame2.NLobby.NTask;

public class OpenInventory : ILobbyTaskCreator
{
	ITask ILobbyTaskCreator.Create(string[] values)
	{
		InvenList.EToggle valueEnum = ArgParser.GetValueEnum(values, 0, InvenList.EToggle.None);
		return SceneAutoBehaviour<InventoryManagement>.instance.CreateOpenInventoryTask(valueEnum);
	}
}
