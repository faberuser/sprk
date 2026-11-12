using System.Collections;
using NGame2.NParty;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NParty;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkRoomShortcut : MonoBehaviour
{
	private static SprkRoomShortcut runner;

	private bool pending;

	public static bool Intercept(InvenItemDesc popup, int action)
	{
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			return false;
		}
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (partyInfo == null || partyInfo.PartyRoom == null)
		{
			return false;
		}
		if (runner == null)
		{
			GameObject obj = new GameObject("SprkRoomShortcut");
			Object.DontDestroyOnLoad(obj);
			runner = obj.AddComponent<SprkRoomShortcut>();
		}
		if (runner.pending)
		{
			return true;
		}
		RaidPartySetting raidPartySetting = null;
		RaidPartySetting[] array = Resources.FindObjectsOfTypeAll<RaidPartySetting>();
		foreach (RaidPartySetting raidPartySetting2 in array)
		{
			if (raidPartySetting2.gameObject.scene.IsValid() && raidPartySetting2.IsOpen())
			{
				raidPartySetting = raidPartySetting2;
				break;
			}
		}
		if (raidPartySetting == null)
		{
			MessagePopup.Open("Item shortcut", "Leave the party room before using this shortcut.");
			return true;
		}
		PartyInfo original = partyInfo;
		RaidPartySetting target = raidPartySetting;
		MessagePopup3Button.Open("Leave raid room?", "This shortcut opens another location. Leave the raid room and continue?", delegate
		{
			if (!runner.pending && !(popup == null) && target.IsOpen() && GlobalAutoBehaviour<PartyManager>.instance.PartyInfo == original)
			{
				runner.pending = true;
				runner.StartCoroutine(runner.LeaveAndContinue(target, popup, action, original));
			}
		}, delegate
		{
		}, "Leave Room", "Stay");
		return true;
	}

	private IEnumerator LeaveAndContinue(RaidPartySetting room, InvenItemDesc popup, int action, PartyInfo original)
	{
		room.Close();
		float deadline = Time.realtimeSinceStartup + 15f;
		while (Time.realtimeSinceStartup < deadline)
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			if (partyInfo == null || partyInfo.PartyRoom == null)
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.ForEach(delegate(IManagementWrapper manager)
				{
					if (!(manager is GenericManagementWrapper<InventoryManagement>))
					{
						manager.CloseAll(immediately: true);
					}
				});
				yield return null;
				pending = false;
				if (popup != null)
				{
					switch (action)
					{
					case 1:
						popup.OnClickMoveDirect();
						break;
					case 2:
						popup.OnClickRestoreSoulStone();
						break;
					default:
						popup.OnClickShortcut();
						break;
					}
				}
				yield break;
			}
			if (partyInfo != original)
			{
				break;
			}
			yield return null;
		}
		pending = false;
		MessagePopup.Open("Item shortcut", "The room could not be left. Please leave the room before using this shortcut.");
	}
}
