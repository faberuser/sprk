using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ChapterTabComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnChapterIndexClick(int chapterIndex);
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/ChapterTabComponent";

	public static PrefabAuto<ChapterTabComponent> PrefabAuto = new PrefabAuto<ChapterTabComponent>("Prefab2/Component/IllustratedBook/ChapterTabComponent");

	private Listener _Listener;

	public UIToggle TagTypeToggle;

	public UILabel TitleLabel;

	public int ChapterIndex;

	public void Init(int chapterIndex, Listener listener)
	{
		ChapterIndex = chapterIndex;
		_Listener = listener;
		if (chapterIndex > GetLastChapterIndex())
		{
			UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("COMMON_ETC"));
		}
		else
		{
			UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect($"COMMON.CHAPTER{ChapterIndex}"));
		}
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
			_Listener.OnChapterIndexClick(ChapterIndex);
		}
	}

	public void SelectToggle(bool On)
	{
		TagTypeToggle.Set(On);
	}

	public bool IsSelected()
	{
		if (TagTypeToggle != null)
		{
			return TagTypeToggle.value;
		}
		return false;
	}

	private int GetLastChapterIndex()
	{
		int num = 0;
		foreach (CampaignChapterData allDatum in GameRoot.Instance.CampaignChapterDataContainer.GetAllData())
		{
			if (allDatum.Type == ChapterType.WorldMap && allDatum.MainField && allDatum.IsOpen)
			{
				num++;
			}
		}
		return num;
	}
}
