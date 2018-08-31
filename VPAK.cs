using System;
using System.Collections;
using System.IO;
using System.Text;

namespace vpak
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class VPak
	{

		struct VPakHeader
		{
			public int Magic, EntryCount, EntryNameOffs, EntryNameLen;
			public void Read(BinaryReader br)
			{
				Magic = br.ReadInt32();
				EntryCount = br.ReadInt32();
				EntryNameOffs = br.ReadInt32();
				EntryNameLen = br.ReadInt32();
			}
			public void Write(BinaryWriter bw)
			{
				bw.Write(Magic);
				bw.Write(EntryCount);
				bw.Write(EntryNameOffs);
				bw.Write(EntryNameLen);
			}
		}

		struct VPakEntry
		{
			public int NameOffs, ELen, NLen, EOffs, NOffs;
			public string Name;
			public void Read(BinaryReader br)
			{
				NameOffs = br.ReadInt32();
				ELen = br.ReadInt32();
				NLen = br.ReadInt32();
				EOffs = br.ReadInt32();
				NOffs = br.ReadInt32();
			}
			public void Write(BinaryWriter bw)
			{
				bw.Write(NameOffs);
				bw.Write(ELen);
				bw.Write(NLen);
				bw.Write(EOffs);
				bw.Write(NOffs);
			}
		}

		static void Unpack(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			BinaryReader br = new BinaryReader(fs);

			VPakHeader header = new VPakHeader();
			header.Read(br);
			VPakEntry[] entries = new VPakEntry[header.EntryCount];
			for(int i=0; i<header.EntryCount; i++)
			{
				entries[i].Read(br);				
			}
			fs.Seek(header.EntryNameOffs, SeekOrigin.Begin);
			byte[] names = br.ReadBytes(header.EntryNameLen);
			Hashtable ht = new Hashtable(header.EntryCount);
			int start = 0;
			string work = "";
			for(int i=0; i<names.Length; i++)
			{
				if (names[i]==0)
				{
					ht.Add(start, work);
					start = i+1;
					work = "";
				} 
				else
				{
					work += (char)names[i];	
				}
			}
			for(int i=0; i<header.EntryCount; i++)
			{
				entries[i].Name = ht[entries[i].NameOffs] as string;
			}

			for(int i=0; i<header.EntryCount; i++)
			{
				Console.WriteLine("   " + entries[i].Name);

				byte[] bin, vlt;

				fs.Seek(entries[i].EOffs, SeekOrigin.Begin);
				bin = br.ReadBytes(entries[i].ELen);

				FileStream fso = new FileStream(entries[i].Name + ".bin", FileMode.Create, FileAccess.Write);
				fso.Write(bin, 0, bin.Length);
				fso.Close();

				fs.Seek(entries[i].NOffs, SeekOrigin.Begin);
				vlt = br.ReadBytes(entries[i].NLen);

				fso = new FileStream(entries[i].Name + ".vlt", FileMode.Create, FileAccess.Write);
				fso.Write(vlt, 0, vlt.Length);
				fso.Close();

			}

			FileInfo fi = new FileInfo(filename);
			string vlsname = fi.Name.Remove(fi.Name.Length - fi.Extension.Length, fi.Extension.Length) + ".vls";
			StreamWriter sw = new StreamWriter(vlsname);
			for(int i=0; i<entries.Length; i++)
			{
				sw.WriteLine(entries[i].Name);
			}
			sw.Close();

			fs.Close();
		}

		static void Pack(string filename)
		{
			
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				//Console.WriteLine("Usage: vpak [-u filename.bin] [-p filename.vls]");
				Console.WriteLine("Usage: vpak [-u filename.bin]");
			} 
			else
			{
				switch(args[0])
				{
					case "-u":
						Console.WriteLine("Unpacking " + args[1] + "...");
						Unpack(args[1]);
						break;
					/*
					case "-p":
						Console.WriteLine("Packing " + args[1] + "...");
						Pack(args[1]);
						break;
					*/
				}
			}
		}
	}
}
