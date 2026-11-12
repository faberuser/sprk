using NGame2.NUI;
using NVespa.NPrefab2;

public class ContentUIGodkingTrialDifficultyNodeComponent : LinkFactoryComponent
{
	public enum LinkDirection
	{
		None,
		Next
	}

	public interface IListener
	{
		void OnClickNode(ContentUIGodkingTrialDifficultyNodeComponent comp);
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDifficultyNodeComponent";

	public UISprite SpriteNodeImg;

	public UISprite SpriteLinkNext;

	public UISprite SpriteSelected;

	public UILabel[] LabelDifficultyList;

	private LinkDirection _linkDir = LinkDirection.Next;

	private int _difficulty = 1;

	private IListener _listener;

	public int Difficulty => _difficulty;

	public void OnClickNode()
	{
		if (_listener != null)
		{
			_listener.OnClickNode(this);
		}
	}

	public void Open(LinkDirection linkDir, int difficulty, IListener listener)
	{
		_listener = listener;
		SetData(linkDir, difficulty);
		UpdateUI();
	}

	public void SetData(LinkDirection linkDir, int difficulty)
	{
		_linkDir = linkDir;
		_difficulty = difficulty;
	}

	public void UpdateUI()
	{
		SetDifficultyText(_difficulty);
		if (!(SpriteLinkNext == null))
		{
			switch (_linkDir)
			{
			case LinkDirection.None:
				UIHelper.EnableGameObject(SpriteLinkNext.gameObject, enable: false);
				break;
			case LinkDirection.Next:
				UIHelper.EnableGameObject(SpriteLinkNext.gameObject, enable: true);
				break;
			}
		}
	}

	public void SetSelect(bool selected)
	{
		UIHelper.EnableGameObject(SpriteSelected.gameObject, selected);
	}

	private void SetDifficultyText(int difficulty)
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(LabelDifficultyList); i < num; i++)
		{
			UIHelper.SetLabelText(LabelDifficultyList[i], difficulty.ToString());
		}
	}
}
