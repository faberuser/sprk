using System;
using System.Collections.Generic;
using NGame2.NScene;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset;

public class GameAssetGui : GlobalOnceBehaviour<GameAssetGui>, IInspectorGuiDrawer
{
	private enum ECounterSort
	{
		None,
		MaxCount,
		MaxDelta
	}

	private class ObjectCounter
	{
		private List<int> _countList = new List<int>();

		public Type Type { get; private set; }

		public int LastCount => _countList[_countList.Count - 1];

		public int LastDelta
		{
			get
			{
				if (_countList.Count < 2)
				{
					return 0;
				}
				int num = _countList[_countList.Count - 1];
				int num2 = _countList[_countList.Count - 2];
				return num - num2;
			}
		}

		public ObjectCounter(Type type)
		{
			Type = type;
			_countList.Add(0);
		}

		public void Next()
		{
			_countList.Add(0);
			if (_countList.Count > 10)
			{
				_countList.RemoveAt(0);
			}
		}

		public void Check(Type checkType)
		{
			if (Type.IsAssignableFrom(checkType))
			{
				_countList[_countList.Count - 1] = _countList[_countList.Count - 1] + 1;
			}
		}

		public string Dump()
		{
			string text = $"{Type.Name}: ";
			for (int num = _countList.Count - 1; num >= 0; num--)
			{
				if (num == 0)
				{
					text += _countList[num];
				}
				else
				{
					int num2 = _countList[num - 1];
					int num3 = _countList[num];
					text += $"{num3}({num3 - num2}) < ";
				}
			}
			return text;
		}
	}

	private ECounterSort _counterSort;

	private IncreaseDic<Type, ObjectCounter> _counterDic = new IncreaseDic<Type, ObjectCounter>();

	private List<string> _namesOfTypeList = new List<string>();

	private int _page;

	private string _bundleAssetSummary = string.Empty;

	private string _currentLockSummary = string.Empty;

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("BundleAssetManager", this, delegate
		{
			if (GUILayout.Button("Dump") && SingletonAuto<BundleAssetManager>.isValidInstance)
			{
				_bundleAssetSummary = SingletonAuto<BundleAssetManager>.instance.GetSummary();
			}
			GUILayout.Label(_bundleAssetSummary);
		});
		GlobalAutoBehaviour<GuiManager>.instance.Add("BundleManager", this, delegate
		{
			if (GUILayout.Button("Dump") && SingletonAuto<BundleManager>.isValidInstance)
			{
				_bundleAssetSummary = SingletonAuto<BundleManager>.instance.GetSummary();
			}
			GUILayout.Label(_bundleAssetSummary);
		});
		GlobalAutoBehaviour<GuiManager>.instance.Add("Lock Count", this, delegate
		{
			if (String_Extension_Method.IsNullOrEmpty(_currentLockSummary))
			{
				if (SingletonAuto<LockContainer>.isValidInstance)
				{
					foreach (LockCounter item in SingletonAuto<LockContainer>.instance.Set)
					{
						if (GUILayout.Button($"{item.Name} (count={item.Count})"))
						{
							_currentLockSummary = item.DumpSummary();
						}
					}
				}
			}
			else
			{
				if (GUILayout.Button("< Back"))
				{
					_currentLockSummary = string.Empty;
				}
				GUILayout.Label(_currentLockSummary);
			}
		});
		GlobalAutoBehaviour<GuiManager>.instance.Add("GameAsset", this, delegate
		{
			bool flag = false;
			ECounterSort eCounterSort = EnumTools.IngamePopup("Sort", _counterSort);
			if (eCounterSort != _counterSort)
			{
				flag = true;
				_counterSort = eCounterSort;
			}
			Define.ReuseAssetInstance.Value = GUILayout.Toggle(Define.ReuseAssetInstance.Value, "Reuse Asset");
			if (GUILayout.Button("Collect", GUILayout.Width(150f), GUILayout.Height(30f)))
			{
				for (int i = 0; i < _counterDic.Count; i++)
				{
					_counterDic.GetValueByIndex(i).Next();
				}
				UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
				{
					for (int j = 0; j < array.Length; j++)
					{
						Type type = array[j].GetType();
						ObjectCounter objectCounter = _counterDic.GETVALUE(type, null);
						if (objectCounter == null)
						{
							objectCounter = new ObjectCounter(type);
							_counterDic.Add(type, objectCounter);
						}
						objectCounter.Check(type);
					}
				}
				flag = true;
			}
			if (flag)
			{
				if (_counterSort == ECounterSort.MaxCount)
				{
					_counterDic.SortByValue((ObjectCounter lhs, ObjectCounter rhs) => rhs.LastCount.CompareTo(lhs.LastCount));
				}
				else if (_counterSort == ECounterSort.MaxDelta)
				{
					_counterDic.SortByValue((ObjectCounter lhs, ObjectCounter rhs) => rhs.LastDelta.CompareTo(lhs.LastDelta));
				}
			}
			LoadingManager.ManualLoading.Value = GUILayout.Toggle(LoadingManager.ManualLoading.Value, "ManualLoading");
			using (new BlockManager().Horizontal())
			{
				using (new BlockManager().Vertical())
				{
					for (int num = 0; num < _counterDic.Count; num++)
					{
						KeyValuePair<Type, ObjectCounter> keyValuePairByIndex = _counterDic.GetKeyValuePairByIndex(num);
						Type key = keyValuePairByIndex.Key;
						ObjectCounter value = keyValuePairByIndex.Value;
						using (new BlockManager().Horizontal())
						{
							if (GUILayout.Button("Detail", GUILayout.Width(80f)))
							{
								_namesOfTypeList.Clear();
								UnityEngine.Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
								if (!__Extension_Method_ICollection__.IsNullOrEmpty(array2))
								{
									for (int num2 = 0; num2 < array2.Length; num2++)
									{
										if (array2[num2].GetType() == key)
										{
											string text = Vespa_Unity_Object_Extension_Method.NAME(array2[num2]);
											if (!String_Extension_Method.IsNullOrEmpty(text))
											{
												_namesOfTypeList.Add(text);
											}
										}
									}
								}
								flag = true;
							}
							GUILayout.Label(value.Dump());
						}
					}
				}
				using (new BlockManager().Vertical())
				{
					int num3 = _namesOfTypeList.Count / 100;
					using (new BlockManager().Horizontal())
					{
						if (GUILayout.Button("<", GUILayout.Width(40f)))
						{
							_page--;
						}
						GUILayout.Label($"{_page}/{num3}", GUILayout.Width(60f));
						if (GUILayout.Button(">", GUILayout.Width(40f)))
						{
							_page++;
						}
					}
					_page = Mathf.Clamp(_page, 0, num3 + 1);
					for (int num4 = 0; num4 < 100; num4++)
					{
						int num5 = num4 + _page * 100;
						if (num5 >= 0)
						{
							if (num5 >= _namesOfTypeList.Count)
							{
								break;
							}
							GUILayout.Label(_namesOfTypeList[num5]);
						}
					}
				}
			}
		});
	}
}
