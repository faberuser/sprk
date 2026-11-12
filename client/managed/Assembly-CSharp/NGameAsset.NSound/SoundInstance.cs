using NVespa;

namespace NGameAsset.NSound;

public class SoundInstance : ISound
{
	public SoundData Data;

	public SoundStock Stock;

	private PoolingList<ClipInstance> _clipList;

	public int DeltaTick
	{
		get
		{
			if (Stock.LocalTimer == null)
			{
				return MainTimer.deltaTick;
			}
			return Stock.LocalTimer.deltaTick;
		}
	}

	string ISound.Code => Data.Code;

	bool ISound.ContainsLoop => Data.ContainsLoop;

	public void Open(SoundData data, SoundStock stock)
	{
		Data = data;
		Stock = stock;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data.ClipList); i++)
		{
			ClipData clipData = data.ClipList[i];
			if (clipData.IsEnableByOption())
			{
				for (int j = 0; j < clipData.BeginTickMs.Length; j++)
				{
					ClipInstance clipInstance = new ClipInstance(this);
					clipInstance.Open(clipData, clipData.BeginTickMs[j], stock.AddVolume);
					_clipList.Add(clipInstance);
				}
			}
		}
	}

	public void Close()
	{
		for (int i = 0; i < _clipList.COUNT(); i++)
		{
			_clipList[i].Close();
		}
		_clipList.Clear();
	}

	void ISound.Update()
	{
		for (int i = 0; i < _clipList.COUNT(); i++)
		{
			_clipList[i].Update();
		}
	}

	bool ISound.IsEnd()
	{
		for (int i = 0; i < _clipList.COUNT(); i++)
		{
			if (_clipList[i].Status != ClipInstance.EStatus.End)
			{
				return false;
			}
		}
		return true;
	}
}
