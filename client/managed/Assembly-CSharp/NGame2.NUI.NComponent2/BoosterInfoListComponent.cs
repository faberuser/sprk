using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class BoosterInfoListComponent : BoosterComponent
{
	public interface Listener
	{
		void OnClickBoosterInfo(BoosterInfoListComponent comp);
	}

	public new const string PrefabPath = "Prefab2/Component/TopMostMenu/BoosterInfoListComponent";

	public new static PrefabAuto<BoosterInfoListComponent> PrefabAuto = new PrefabAuto<BoosterInfoListComponent>("Prefab2/Component/TopMostMenu/BoosterInfoListComponent");

	private Listener _listener;

	public UIButton ButtonMove;

	public UILabel LabelBoosterName;

	public UISprite SpriteNew;

	public void Open(Listener listener, BoosterInfo info)
	{
		Reset();
		_listener = listener;
		Open(info);
		UIHelper.SetLabelText(LabelBoosterName, info.Name);
		UIHelper.EnableGameObject(UiEffect_Border, enable: false);
		bool enable = info.MoveTaskType != ClientActivityType.None;
		UIHelper.EnableGameObject(ButtonMove, enable);
	}

	public void SetNewSprite(bool visible)
	{
		UIHelper.EnableGameObject(SpriteNew, visible);
	}

	public void OnClickBoosterInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickBoosterInfo(this);
		}
	}

	private void Reset()
	{
		_listener = null;
		SetNewSprite(visible: false);
	}
}
