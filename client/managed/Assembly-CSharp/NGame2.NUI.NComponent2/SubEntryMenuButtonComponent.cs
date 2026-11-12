using System;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NComponent2;

public class SubEntryMenuButtonComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(SubEntryMenuButtonComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/GameOption/SubEntryMenuButtonComponent";

	public static PrefabAuto<SubEntryMenuButtonComponent> PrefabAuto = new PrefabAuto<SubEntryMenuButtonComponent>("Prefab2/Component/GameOption/SubEntryMenuButtonComponent");

	public UILabel label;

	protected Action _openEntryMenu;

	protected Action _onClickDepth1;

	protected Action _onClickDepth2;

	private Listener _listener;

	public Type ActionType { get; private set; }

	public void Open(Listener listener, EntryMenu entryMenu, Type actionType, Action openEntryMenu, Action onClickDepth1, Action onClickDepth2)
	{
		ActionType = actionType;
		_listener = listener;
		_openEntryMenu = openEntryMenu;
		_onClickDepth1 = onClickDepth1;
		_onClickDepth2 = onClickDepth2;
		label.text = LocalizationManager.GetStringDirect(entryMenu.EntryMenuButton[0]);
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
		Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask(), _onClickDepth2).Commit();
	}
}
