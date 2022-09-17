using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BRY
{
#pragma warning disable CS8600

	/// <summary>
	/// ConsoleアプリのMainの中で使う
	/// アプリの呼び出し用
	/// </summary>
	internal class CallExe
	{
		// ************************************************************************
		public enum StartupOption
		{
			None = 0,
			IsRunning,
			Call
		}
		// ************************************************************************
		private string m_CallExeName = "m2p";
		/// <summary>
		/// 実行するアプリの名前
		/// </summary>
		public string CallExeName { get { return m_CallExeName; } }
		private int m_Result = 0;
		/// <summary>
		/// 実行後のリザルトコード 1:正常 2;なんかあり
		/// </summary>
		public int Result { get { return m_Result; } }
		private string m_ResultString = "false";
		/// <summary>
		/// 実行後のステータス文字 true or false
		/// </summary>
		public string ResultString { get { return m_ResultString; } }
		// ************************************************************************
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		// ************************************************************************
		public CallExe(string callExeName)
		{
			m_CallExeName = callExeName;
			m_Result = 0;
		}
		// ************************************************************************
		/// <summary>
		/// 実行部
		/// </summary>
		/// <param name="args">Mainのargs</param>
		/// <returns>実行コード</returns>
		public int Run(string[] args)
		{
			int ret = 0;
			string rets = "";

			StartupOption so = GetOption(args);
			// call or isRunning
			if (so != StartupOption.None)
			{
				Process proc = null;
				Process[] ps = Process.GetProcessesByName(CallExeName);
				bool isRunnig = (ps.Length > 0);
				if (isRunnig) proc = ps[0];
				switch (so)
				{
					// 起動してるか確認するだけ
					case StartupOption.IsRunning:
						if (isRunnig)
						{
							rets = "true";
							ret = 1;
						}
						else
						{
							rets = "false";
							ret = 0;
						}
						break;
					// アプリを実行する
					// 実行してるなら全面に
					case StartupOption.Call:
						if (isRunnig)
						{
							if (proc != null)
								SetForegroundWindow(proc.MainWindowHandle);
						}
						else
						{
							Process exec = new Process();
							exec.StartInfo.FileName = CallExePath(CallExeName);
							exec.StartInfo.Arguments = "";// ArgsString(args);
							if (exec.Start())
							{
								ret = 1;
								rets = "true";
							}
							else
							{
								ret = 0;
								rets = "false";
							}
						}
						break;
				}
			}
			else
			{
				// アプリを引数付きで呼び出す。
				string exename = CallExePath(CallExeName);
				ret = 0;
				rets = "false";
				if (File.Exists(exename) == true)
				{
					Process exec2 = new Process();
					exec2.StartInfo.FileName = exename;
					exec2.StartInfo.Arguments = ArgsString(args);
					try
					{
						if (exec2.Start())
						{
							ret = 1;
							rets = "true";
						}

					}
					catch
					{
						//
					}
				}
			}
			m_Result = ret;
			m_ResultString = rets;

			return ret;
		}
		// ************************************************************************
		/// <summary>
		/// 実行するファイルのフルパス。
		/// このコンソールアプリと同じフォルダにあること
		/// </summary>
		/// <param name="nm"></param>
		/// <returns></returns>
		private string CallExePath(string nm)
		{
			string ret = "";
			string fullName = Environment.ProcessPath;
			string n = "";
			if (fullName != null)
			{
				n = Path.GetDirectoryName(fullName);

			}
			if ((n != null) && (n != ""))
			{
				ret = Path.Combine(n, nm + ".exe");
			}
			else
			{
				ret = nm + ".exe";
			}
			return ret;
		}
		// ************************************************************************
		/// <summary>
		/// argsを解析　call or isRunning　or その他　
		/// </summary>
		/// <param name="args">Mainのargs</param>
		/// <returns></returns>
		private StartupOption GetOption(string[] args)
		{
			StartupOption ret = StartupOption.None;
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if ((args[i][0] == '-') || (args[i][0] == '/'))
					{
						string p = args[i].Substring(1).ToLower();
						switch (p)
						{
							case "isrunning":
							case "exenow":
							case "execnow":
								ret = StartupOption.IsRunning;
								break;
							case "call":
							case "execute":
							case "start":
								ret = StartupOption.Call;
								break;
						}

					}
				}
			}
			return ret;
		}
		// ************************************************************************
		/// <summary>
		/// Mainのargsを一つの文字列に変換
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private string ArgsString(string[] args)
		{
			string opts = "";
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (opts != "") opts += " ";
					opts += "\"" + args[i] + "\"";
				}
			}
			return opts;
		}
	}
}
/*
 　　// 使用サンプル
	internal class Program
	{
		public static string CallExeName = "m2p";
		// ************************************************************************
		static int Main(string[] args)
		{
			CallExe ce = new CallExe(CallExeName);
			ce.Run(args);

			Console.WriteLine(ce.ResultString);
			return ce.Result;
		}
	}
 */
#pragma warning restore CS8600
