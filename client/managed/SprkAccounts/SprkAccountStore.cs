using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

public sealed class SprkAccountStore
{
	public const int MaxAccounts = 4;

	private readonly string path;

	public readonly List<SprkSavedAccount> Accounts = new List<SprkSavedAccount>();

	public static string Endpoint(string value)
	{
		return value.TrimEnd('/');
	}

	public SprkAccountStore(string file)
	{
		path = file;
		Reload();
	}

	private List<SprkSavedAccount> Read()
	{
		List<SprkSavedAccount> list = new List<SprkSavedAccount>();
		if (!File.Exists(path))
		{
			return list;
		}
		try
		{
			using BinaryReader binaryReader = new BinaryReader(new MemoryStream(ProtectedData.Unprotect(File.ReadAllBytes(path), null, DataProtectionScope.CurrentUser)), Encoding.UTF8);
			if (binaryReader.ReadInt32() != 1)
			{
				throw new InvalidDataException();
			}
			int num = binaryReader.ReadInt32();
			if (num < 0 || num > 10000)
			{
				throw new InvalidDataException();
			}
			for (int i = 0; i < num; i++)
			{
				SprkSavedAccount sprkSavedAccount = new SprkSavedAccount
				{
					Username = binaryReader.ReadString(),
					ServerName = binaryReader.ReadString(),
					Endpoint = binaryReader.ReadString(),
					RefreshToken = binaryReader.ReadString()
				};
				if (string.IsNullOrEmpty(sprkSavedAccount.Username) || string.IsNullOrEmpty(sprkSavedAccount.Endpoint) || string.IsNullOrEmpty(sprkSavedAccount.RefreshToken))
				{
					throw new InvalidDataException();
				}
				list.Add(sprkSavedAccount);
			}
		}
		catch (CryptographicException)
		{
			list.Clear();
		}
		catch (InvalidDataException)
		{
			list.Clear();
		}
		catch (EndOfStreamException)
		{
			list.Clear();
		}
		return list;
	}

	public bool Reload()
	{
		List<SprkSavedAccount> list = Read();
		bool flag = list.Count != Accounts.Count;
		if (!flag)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!Same(list[i], Accounts[i]) || list[i].RefreshToken != Accounts[i].RefreshToken)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Accounts.Clear();
			Accounts.AddRange(list);
		}
		return flag;
	}

	public static bool Same(SprkSavedAccount a, SprkSavedAccount b)
	{
		if (a.Username == b.Username && a.Endpoint == b.Endpoint)
		{
			return a.ServerName == b.ServerName;
		}
		return false;
	}

	public bool CanSave(SprkSavedAccount account)
	{
		if (Accounts.Count >= 4)
		{
			return Accounts.Exists((SprkSavedAccount old) => Same(old, account));
		}
		return true;
	}

	private FileStream LockStore()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		int num = 0;
		while (true)
		{
			try
			{
				return new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				if (num >= 100)
				{
					throw;
				}
				Thread.Sleep(10);
			}
			num++;
		}
	}

	public IDisposable TryAcquire(SprkSavedAccount account)
	{
		string text;
		using (SHA256 sHA = SHA256.Create())
		{
			text = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(account.Endpoint + "\n" + account.ServerName + "\n" + account.Username))).Replace("-", "");
		}
		string path = this.path + ".sessions";
		Directory.CreateDirectory(path);
		try
		{
			return new FileStream(Path.Combine(path, text + ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}
		catch (IOException)
		{
			return null;
		}
	}

	public bool InUse(SprkSavedAccount account)
	{
		using IDisposable disposable = TryAcquire(account);
		return disposable == null;
	}

	public void Upsert(SprkSavedAccount account)
	{
		using (LockStore())
		{
			List<SprkSavedAccount> list = Read();
			if (!list.Exists((SprkSavedAccount old) => Same(old, account)) && list.Count >= 4)
			{
				throw new InvalidOperationException("Four accounts are already saved. Log out of an account before adding another.");
			}
			list.RemoveAll((SprkSavedAccount old) => Same(old, account));
			list.Insert(0, account);
			Save(list);
		}
	}

	public void Remove(SprkSavedAccount account)
	{
		using (LockStore())
		{
			List<SprkSavedAccount> list = Read();
			list.RemoveAll((SprkSavedAccount old) => Same(old, account));
			Save(list);
		}
	}

	private void Save(List<SprkSavedAccount> next)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
			{
				binaryWriter.Write(1);
				binaryWriter.Write(next.Count);
				foreach (SprkSavedAccount item in next)
				{
					binaryWriter.Write(item.Username);
					binaryWriter.Write(item.ServerName);
					binaryWriter.Write(item.Endpoint);
					binaryWriter.Write(item.RefreshToken);
				}
			}
			array = memoryStream.ToArray();
		}
		byte[] bytes = ProtectedData.Protect(array, null, DataProtectionScope.CurrentUser);
		Array.Clear(array, 0, array.Length);
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		string sourceFileName = path + ".tmp";
		try
		{
			File.WriteAllBytes(sourceFileName, bytes);
			if (File.Exists(path))
			{
				File.Replace(sourceFileName, path, null);
			}
			else
			{
				File.Move(sourceFileName, path);
			}
			Accounts.Clear();
			Accounts.AddRange(next);
		}
		finally
		{
			if (File.Exists(sourceFileName))
			{
				File.Delete(sourceFileName);
			}
		}
	}
}
