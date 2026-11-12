using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class PetGradeTabComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickTab(CreatureGrade grade);
	}

	public const string PrefabPath = "Prefab2/Component/Pet/PetGradeTabComponent";

	public static PrefabAuto<PetGradeTabComponent> PrefabAuto = new PrefabAuto<PetGradeTabComponent>("Prefab2/Component/Pet/PetGradeTabComponent");

	private IListener _Listener;

	public UIToggle TagTypeToggle;

	public UISpriteSelector SpriteSelector;

	public UILabel TitleLabel;

	public UISprite DisabledSprite;

	public UISprite NewSprite;

	public CreatureGrade GradeType { get; protected set; }

	public void Init(CreatureGrade gradeType, IListener listener)
	{
		GradeType = gradeType;
		_Listener = listener;
		if (SpriteSelector != null)
		{
			SpriteSelector.Select(GradeType.ToString());
		}
		string text = UIHelper.GradeLocaleText((byte)gradeType);
		UIHelper.SetLabelText(TitleLabel, text);
	}

	public void SetNew(bool show)
	{
		UIHelper.EnableGameObject(NewSprite, show);
	}

	public void SetDisabled(bool show)
	{
		UIHelper.EnableGameObject(DisabledSprite, show);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public virtual void OnClickToggle()
	{
		if (!(TagTypeToggle == null) && TagTypeToggle.value && _Listener != null)
		{
			_Listener.OnClickTab(GradeType);
		}
	}

	public void SelectToggle(bool On)
	{
		TagTypeToggle.Set(On);
	}
}
