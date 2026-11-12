using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NShared.NUtil;

public class Aes128
{
	public static string CreateSalt(int size)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[size];
		rNGCryptoServiceProvider.GetBytes(array);
		return Convert.ToBase64String(array);
	}

	public string DecryptTextLegacy(string textToDecrypt, string key)
	{
		byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
		byte[] bytes = DecryptBinary(encryptedData, key);
		return Encoding.UTF8.GetString(bytes);
	}

	public byte[] DecryptBinary(byte[] encryptedData, string key)
	{
		RijndaelManaged obj = new RijndaelManaged
		{
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7,
			KeySize = 128,
			BlockSize = 128
		};
		byte[] array = Convert.FromBase64String(key);
		byte[] array2 = new byte[16];
		int num = array.Length;
		if (num > array2.Length)
		{
			num = array2.Length;
		}
		Array.Copy(array, array2, num);
		obj.Key = array2;
		obj.IV = array2;
		return obj.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
	}

	public string EncryptTextLegacy(string textToEncrypt, string key)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(textToEncrypt);
		return Convert.ToBase64String(EncryptBinary(bytes, key));
	}

	public byte[] EncryptTextLegacy(byte[] bytes, string key)
	{
		string s = Encoding.UTF8.GetString(bytes);
		byte[] bytes2 = Encoding.UTF8.GetBytes(s);
		string s2 = Convert.ToBase64String(EncryptBinary(bytes2, key));
		return Encoding.UTF8.GetBytes(s2);
	}

	public byte[] EncryptText(byte[] bytes, string key)
	{
		string s = Convert.ToBase64String(bytes);
		byte[] bytes2 = Encoding.UTF8.GetBytes(s);
		return EncryptBinary(bytes2, key);
	}

	public byte[] EncryptBinary(byte[] bytes, string key)
	{
		RijndaelManaged obj = new RijndaelManaged
		{
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7,
			KeySize = 128,
			BlockSize = 128
		};
		byte[] array = Convert.FromBase64String(key);
		byte[] array2 = new byte[16];
		int num = array.Length;
		if (num > array2.Length)
		{
			num = array2.Length;
		}
		Array.Copy(array, array2, num);
		obj.Key = array2;
		obj.IV = array2;
		return obj.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
	}

	public byte[] EncryptFixedSize(byte[] bytes, string key)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.ECB;
		rijndaelManaged.Padding = PaddingMode.None;
		rijndaelManaged.KeySize = 128;
		rijndaelManaged.BlockSize = 128;
		byte[] array = Convert.FromBase64String(key);
		byte[] array2 = new byte[16];
		int num = array.Length;
		if (num > array2.Length)
		{
			num = array2.Length;
		}
		Array.Copy(array, array2, num);
		rijndaelManaged.Key = array2;
		rijndaelManaged.IV = array2;
		ICryptoTransform transform = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			cryptoStream.Close();
		}
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public byte[] DecryptFixedSize(byte[] bytes, string key)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.ECB;
		rijndaelManaged.Padding = PaddingMode.None;
		rijndaelManaged.KeySize = 128;
		rijndaelManaged.BlockSize = 128;
		byte[] array = Convert.FromBase64String(key);
		byte[] array2 = new byte[16];
		int num = array.Length;
		if (num > array2.Length)
		{
			num = array2.Length;
		}
		Array.Copy(array, array2, num);
		rijndaelManaged.Key = array2;
		rijndaelManaged.IV = array2;
		ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			cryptoStream.Close();
		}
		memoryStream.Close();
		return memoryStream.ToArray();
	}
}
