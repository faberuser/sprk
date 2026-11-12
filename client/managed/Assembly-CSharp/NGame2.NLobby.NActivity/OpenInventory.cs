using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NInventory;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenInventory : IActivity
{
	private InvenList.EToggle _toggle;

	private string _craftItemCode;

	public static IActivity Create(string[] values)
	{
		InvenList.EToggle valueEnum = ArgParser.GetValueEnum(values, 0, InvenList.EToggle.None);
		string craftItemCode = null;
		if (values.Length > 1)
		{
			craftItemCode = values[1];
		}
		return new OpenInventory(valueEnum, craftItemCode);
	}

	public OpenInventory(InvenList.EToggle toggle, string craftItemCode = null)
	{
		_toggle = toggle;
		if (craftItemCode != null)
		{
			_craftItemCode = craftItemCode;
		}
	}

	IEnumerator IActivity.coStart()
	{
		if (_craftItemCode != null)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenCraftWindow(_craftItemCode);
		}
		else
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory(_toggle);
		}
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
