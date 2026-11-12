using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Saved Option")]
public class UISavedOption : MonoBehaviour
{
	public string keyName;

	private UIPopupList mList;

	private UIToggle mCheck;

	private UIProgressBar mSlider;

	private string key
	{
		get
		{
			if (!string.IsNullOrEmpty(keyName))
			{
				return keyName;
			}
			return "NGUI State: " + base.name;
		}
	}

	private void Awake()
	{
		mList = GetComponent<UIPopupList>();
		mCheck = GetComponent<UIToggle>();
		mSlider = GetComponent<UIProgressBar>();
	}

	private void OnEnable()
	{
		if (mList != null)
		{
			EventDelegate.Add(mList.onChange, SaveSelection);
			string value = SprkTestPrefs.GetString(key);
			if (!string.IsNullOrEmpty(value))
			{
				mList.value = value;
			}
			return;
		}
		if (mCheck != null)
		{
			EventDelegate.Add(mCheck.onChange, SaveState);
			mCheck.value = SprkTestPrefs.GetInt(key, mCheck.startsActive ? 1 : 0) != 0;
			return;
		}
		if (mSlider != null)
		{
			EventDelegate.Add(mSlider.onChange, SaveProgress);
			mSlider.value = SprkTestPrefs.GetFloat(key, mSlider.value);
			return;
		}
		string text = SprkTestPrefs.GetString(key);
		UIToggle[] componentsInChildren = GetComponentsInChildren<UIToggle>(includeInactive: true);
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			UIToggle obj = componentsInChildren[i];
			obj.value = obj.name == text;
		}
	}

	private void OnDisable()
	{
		if (mCheck != null)
		{
			EventDelegate.Remove(mCheck.onChange, SaveState);
			return;
		}
		if (mList != null)
		{
			EventDelegate.Remove(mList.onChange, SaveSelection);
			return;
		}
		if (mSlider != null)
		{
			EventDelegate.Remove(mSlider.onChange, SaveProgress);
			return;
		}
		UIToggle[] componentsInChildren = GetComponentsInChildren<UIToggle>(includeInactive: true);
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			UIToggle uIToggle = componentsInChildren[i];
			if (uIToggle.value)
			{
				SprkTestPrefs.SetString(key, uIToggle.name);
				break;
			}
		}
	}

	public void SaveSelection()
	{
		SprkTestPrefs.SetString(key, UIPopupList.current.value);
	}

	public void SaveState()
	{
		SprkTestPrefs.SetInt(key, UIToggle.current.value ? 1 : 0);
	}

	public void SaveProgress()
	{
		SprkTestPrefs.SetFloat(key, UIProgressBar.current.value);
	}
}
