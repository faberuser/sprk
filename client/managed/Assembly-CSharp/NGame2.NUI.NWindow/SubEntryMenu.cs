using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow;

public class SubEntryMenu : EntryMenu
{
	public new interface Listener : EntryMenu.Listener
	{
		void OnCloseSubMenu();
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/EntryMenu/SubEntryMenu";

	protected Listener _SubListener => _listener as Listener;

	public void Init(Listener listener, CampaignNodeData nodeData, int parentMenuIndex)
	{
		if (nodeData != null && !String_Extension_Method.IsNullOrEmpty(nodeData.EntryNPCCode))
		{
			_NPCData = GameRoot.Instance.NPCDataContainer.GetData(nodeData.EntryNPCCode);
			if (_NPCData != null)
			{
				_nodeData = nodeData;
				_EntryNPCCode = nodeData.EntryNPCCode;
				_listener = listener;
				SetupMenuButtonGroup(parentMenuIndex);
			}
		}
	}

	protected void SetupMenuButtonGroup(int parentMenuIndex)
	{
		if (_NPCData != null)
		{
			int num = 1;
			while (true)
			{
				NShared.EntryMenu entryMenuButton = _NPCData.GetEntryMenuButton(num);
				if (entryMenuButton == null || !entryMenuButton.IsValid)
				{
					break;
				}
				if (entryMenuButton.IsChild && entryMenuButton.ParentIndex == parentMenuIndex && CheckCondition(entryMenuButton.EntryMenuCondition))
				{
					SetupMenuButton(entryMenuButton);
				}
				num++;
			}
		}
		RepositionTableGrid();
	}

	protected override void SetupMenuButton(NShared.EntryMenu menu)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 1);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action == null)
		{
			return;
		}
		SubEntryMenuComponent subEntryMenuComponent = SubEntryMenuComponent.PrefabAuto.Factory.Create(TableGrid.transform);
		if (_nodeData != null)
		{
			subEntryMenuComponent.Open(_NPCData, menu, action.GetType(), delegate
			{
				action.Run(_nodeData, menu);
			});
		}
		else
		{
			subEntryMenuComponent.Open(_NPCData, menu, action.GetType(), delegate
			{
				action.Run(menu, new List<object> { _EntryNPCCode });
			});
		}
		_ListButtons.Add(subEntryMenuComponent);
	}

	public override void OnClickCloseButton()
	{
		if (_SubListener != null)
		{
			_SubListener.OnCloseSubMenu();
		}
		Close();
	}

	public override void ReOpen()
	{
	}
}
