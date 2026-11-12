using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NVespa;

public class VespaProfiler : SingletonAuto<VespaProfiler>
{
	public class Block : IDisposable
	{
		public Block(string name)
		{
			SingletonAuto<VespaProfiler>.instance.Start(name);
		}

		void IDisposable.Dispose()
		{
			SingletonAuto<VespaProfiler>.instance.End();
		}
	}

	public class DurationBlock : IDisposable
	{
		private string _name;

		public DurationBlock(string name)
		{
			_name = name;
			SingletonAuto<VespaProfiler>.instance.StartDuration(name);
		}

		void IDisposable.Dispose()
		{
			SingletonAuto<VespaProfiler>.instance.EndDuration(_name);
		}
	}

	private class Item
	{
		public float StartTime;

		public ProfileData Profile;

		public Item(ProfileData profile)
		{
			StartTime = Time.realtimeSinceStartup;
			Profile = profile;
		}

		public void End()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			Profile.Add(StartTime, realtimeSinceStartup);
		}
	}

	public class ProfileData
	{
		public float MinTime;

		public float MaxTime;

		public float AverageTime;

		public int Count;

		private IncreaseDic<string, ProfileData> _childDic;

		public string Name { get; private set; }

		public IncreaseDic<string, ProfileData> ChildDic
		{
			get
			{
				if (_childDic == null)
				{
					_childDic = new IncreaseDic<string, ProfileData>();
				}
				return _childDic;
			}
		}

		public ProfileData(string name)
		{
			Name = name;
		}

		public void Sort()
		{
			if (_childDic != null)
			{
				ChildDic.SortByValue((ProfileData lhs, ProfileData rhs) => rhs.AverageTime.CompareTo(lhs.AverageTime));
				for (int num = 0; num < ChildDic.Count; num++)
				{
					ChildDic.GetValueByIndex(num).Sort();
				}
			}
		}

		public void Dump(StringBuilder sb, int depth = 0)
		{
			sb.Append(' ', depth * 5);
			sb.AppendLine($"{Name}(avg={AverageTime:F5}, min={MinTime:F5}, max={MaxTime:F5}, count={Count})");
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_childDic); i++)
			{
				ChildDic.GetValueByIndex(i).Dump(sb, depth + 1);
			}
			if (depth < 2)
			{
				sb.AppendLine();
			}
		}

		public ProfileData GetChild(string name)
		{
			ProfileData profileData = ChildDic.GETVALUE(name, null);
			if (profileData == null)
			{
				profileData = new ProfileData(name);
				ChildDic.Add(name, profileData);
			}
			return profileData;
		}

		public void Add(float beginTime, float endTime)
		{
			float num = endTime - beginTime;
			if (Count == 0)
			{
				MinTime = num;
				MaxTime = num;
				AverageTime = num;
			}
			else
			{
				MinTime = Mathf.Min(MinTime, num);
				MaxTime = Mathf.Max(MaxTime, num);
				AverageTime = (AverageTime * (float)Count + num) / (float)(Count + 1);
			}
			Count++;
		}
	}

	public static LocalStorageBool Enabled = new LocalStorageBool(i_default: false, typeof(VespaProfiler).FullName + "Enabled");

	public IncreaseDic<string, ProfileData> DurationDic = new IncreaseDic<string, ProfileData>();

	private Dictionary<string, Item> _durationItemDic = new Dictionary<string, Item>();

	private Stack<Item> _stack = new Stack<Item>();

	private int _frameCount = -1;

	public ProfileData Root { get; private set; }

	public VespaProfiler()
	{
		Root = new ProfileData("Root");
	}

	public void Clear()
	{
		while (_stack.Count > 0)
		{
			End();
		}
		Root.ChildDic.Clear();
		DurationDic.Clear();
		_durationItemDic.Clear();
	}

	public void Dump(StringBuilder sb)
	{
		Root.Dump(sb);
		if (!DurationDic.IsNullOrEmpty())
		{
			sb.AppendLine("--- Duration ---");
			for (int i = 0; i < DurationDic.Count; i++)
			{
				DurationDic.GetValueByIndex(i).Dump(sb);
			}
		}
	}

	public void Start(string name)
	{
		if (Enabled.Value)
		{
			RefreshFrameCount();
			Item item = new Item(((_stack.Count > 0) ? _stack.Peek().Profile : Root).GetChild(name));
			_stack.Push(item);
		}
	}

	public void End()
	{
		if (Enabled.Value && _stack.Count != 0)
		{
			_stack.Pop().End();
		}
	}

	public void StartDuration(string name)
	{
		if (Enabled.Value)
		{
			ProfileData profileData = DurationDic.GETVALUE(name, null);
			if (profileData == null)
			{
				profileData = new ProfileData(name);
				DurationDic.Add(name, profileData);
			}
			_durationItemDic.Remove(name);
			Item value = new Item(profileData);
			_durationItemDic.Add(name, value);
		}
	}

	public void EndDuration(string name)
	{
		if (Enabled.Value)
		{
			Item vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_durationItemDic, name, null);
			if (vALUE != null)
			{
				vALUE.End();
				_durationItemDic.Remove(name);
			}
		}
	}

	private void RefreshFrameCount()
	{
		int frameCount = Time.frameCount;
		if (_frameCount != frameCount)
		{
			_frameCount = frameCount;
			while (_stack.Count > 0)
			{
				End();
			}
		}
	}
}
