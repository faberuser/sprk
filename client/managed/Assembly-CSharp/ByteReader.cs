using System.IO;
using System.Text;
using UnityEngine;

public class ByteReader : ILineReader
{
	private byte[] mBuffer;

	private int mOffset;

	public bool canRead
	{
		get
		{
			if (mBuffer != null)
			{
				return mOffset < mBuffer.Length;
			}
			return false;
		}
	}

	public ByteReader(byte[] bytes)
	{
		mBuffer = bytes;
	}

	public ByteReader(TextAsset asset)
	{
		mBuffer = asset.bytes;
	}

	public static ByteReader Open(string path)
	{
		FileStream fileStream = File.OpenRead(path);
		if (fileStream != null)
		{
			fileStream.Seek(0L, SeekOrigin.End);
			byte[] array = new byte[fileStream.Position];
			fileStream.Seek(0L, SeekOrigin.Begin);
			fileStream.Read(array, 0, array.Length);
			fileStream.Close();
			return new ByteReader(array);
		}
		return null;
	}

	private static string ReadLine(byte[] buffer, int start, int count)
	{
		return Encoding.UTF8.GetString(buffer, start, count);
	}

	public string ReadLine()
	{
		return ReadLine(skipEmptyLines: true);
	}

	public string ReadLine(bool skipEmptyLines)
	{
		int num = mBuffer.Length;
		if (skipEmptyLines)
		{
			while (mOffset < num && mBuffer[mOffset] < 32)
			{
				mOffset++;
			}
		}
		int num2 = mOffset;
		if (num2 < num)
		{
			int num3;
			do
			{
				if (num2 < num)
				{
					num3 = mBuffer[num2++];
					continue;
				}
				num2++;
				break;
			}
			while (num3 != 10 && num3 != 13);
			string result = ReadLine(mBuffer, mOffset, num2 - mOffset - 1);
			mOffset = num2;
			return result;
		}
		mOffset = num;
		return null;
	}
}
