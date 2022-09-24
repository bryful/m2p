﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using static System.Net.Mime.MediaTypeNames;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace BRY
{
#pragma warning disable CS8600
#pragma warning disable CS8601 // Null 参照代入の可能性があります。
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。

	/// <summary>
	/// どこから送られたか
	/// </summary>
	public enum PIPECALL
	{
		/// <summary>
		/// 最初の起動コマンド
		/// </summary>
		StartupExec,
		/// <summary>
		/// 起動中に送られてきた
		/// </summary>
		DoubleExec,
		/// <summary>
		/// Pipeで送られてきた
		/// </summary>
		PipeExec,
		None
	}

	/// <summary>
	/// Pipeで送受信する時のJsonデータを扱うクラス
	/// </summary>
	public class PipeData
	{
		private JsonObject? obj = new JsonObject();

		public string[] Args { get { return GetArgs(); } set { SetArgs(value); } }
		public PIPECALL PIPECALL { get { return GetPIPECALL(); } set { SetPIPECALL(value); } }
		public PipeData(string [] args,PIPECALL pc)
		{
			SetArgs(args);
			SetPIPECALL(pc);
		}
		// --------------------------------
		public PipeData(string js)
		{
			FromJson(js);
		}
		// --------------------------------
		/// <summary>
		/// Jsonからデータを得る
		/// </summary>
		/// <param name="js"></param>
		public void FromJson(string js)
		{
			try
			{
				var doc = JsonNode.Parse(js);
				obj = (JsonObject)doc;
			}
			catch
			{
				obj = new JsonObject();
			}
		}
		// --------------------------------
		/// <summary>
		/// Jsonへ
		/// </summary>
		/// <returns></returns>
		public string ToJson()
		{
			return obj.ToJsonString();
		}
		// --------------------------------
		public void SetPIPECALL(PIPECALL pc)
		{
			int vv = (int)pc;
			obj.Add("PIPECALL", vv);
		}
		// --------------------------------
		public void SetArgs(string[] args)
		{
			JsonArray ja = new JsonArray();
			if(args.Length>0)
			{
				foreach(string arg in args)
				{
					ja.Add(arg);
				}
			}
			obj.Add("Args", ja);
		}
		// --------------------------------
		public string[] GetArgs()
		{
			string[] ret = new string[0];
			try
			{
				var ja = obj?["Args"].AsArray();
				if(ja.Count>0)
				{
					ret = new string[ja.Count];
					int i = 0;
					foreach(var item in ja)
					{
						ret[i] = item.GetValue<string>();
						i++;
					}
				}
			}
			catch
			{
				ret = new string[0];
			}
			return ret;
		}
		// --------------------------------
		// ****************************************************
		public PIPECALL GetPIPECALL()
		{
			PIPECALL ret = PIPECALL.None;
			try
			{
				int i = obj["PIPECALL"].GetValue<int>();
				ret = (PIPECALL)i;
			}
			catch
			{
				ret = PIPECALL.None;
			}
			return ret;
		}
	}

	/// <summary>
	/// ConsoleアプリのMainの中で使う
	/// アプリの呼び出し用
	/// </summary>
	public class CallExe
	{
		// ************************************************************************
		public enum StartupOption
		{
			None = 0,
			IsRunning,
			Call
		}
		// ************************************************************************
		private string m_AppID = "SkeltonWinForm";
		private string m_MyID = "SkeltonWinFormCall";
		/// <summary>
		/// 実行するアプリの名前
		/// </summary>
		public string AppID { get { return m_AppID; } }
		/// <summary>
		/// 自分自身のアプリの名前
		/// </summary>
		public string MyID { get { return m_MyID; } }
		private int m_Result = 0;
		/// <summary>
		/// 実行後のリザルトコード 1:正常 2;なんかあり
		/// </summary>
		public int Result { get { return m_Result; } }
		private string m_ResultString = "false";
		/// <summary>
		/// 実行後に
		/// </summary>
		public string ResultString { get { return m_ResultString; } }
		// ************************************************************************
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		// ************************************************************************
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ap">呼び出すアプリ名</param>
		/// <param name="my">自分自身のアプリ名</param>
		public CallExe(string ap,string my)
		{
			m_AppID = ap;
			m_MyID = my;
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
			string rets = "false";
			System.Threading.Mutex _mutex = new System.Threading.Mutex(false, m_AppID);
			bool isRunnig = !(_mutex.WaitOne(0, false));
			Process proc = null;
			if(isRunnig)
			{
				Process[] ps = Process.GetProcessesByName(AppID);
				if(ps.Length > 0) proc = ps[0];
			}
			StartupOption so = GetOption(args);
			switch (so)
			{
				case StartupOption.IsRunning:
					if (isRunnig)
					{
						rets = "true";
						ret = 1;
					}
					break;
				// アプリを実行する
				// 実行してるなら全面に
				case StartupOption.Call:
					if (isRunnig)
					{
						if (proc != null) {
							SetForegroundWindow(proc.MainWindowHandle);
							rets = "true";
							ret = 1;
						}
					}
					else
					{
						Process exec = new Process();
						exec.StartInfo.FileName = CallExePath(AppID);
						exec.StartInfo.Arguments = "";// ArgsString(args);
						if (exec.Start())
						{
							ret = 1;
							rets = "true";
						}
					}
					break;
				case StartupOption.None:
				default:
					if (isRunnig)
					{
						_execution = true;
						_Result = "";
						MessagePipeServer(m_MyID);
						PipeData pd = new PipeData(args, PIPECALL.PipeExec);
						string s = pd.ToJson();
						PipeClient(m_AppID, s).Wait();
						int cnt = 0;
						while(_execution)
						{
							Thread.Sleep(300);
							cnt++;
							if (cnt >= 50) { _execution = false; }
						}
						ret = 1;
						rets = _Result;
					}
					else
					{
						Process exec2 = new Process();
						exec2.StartInfo.FileName = CallExePath(AppID);
						exec2.StartInfo.Arguments = ArgsString(args);
						if (exec2.Start())
						{
							ret = 1;
							rets = "true";
						}
					}
					break;
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
		// **************************************************************************
		public static bool _execution = true;
		public static string _Result= "";
		/// <summary>
		/// メッセージを受け取る。受け取ったら終了する。
		/// </summary>
		/// <param name="pipeName"></param>
		static public void MessagePipeServer(string pipeName)
		{
			Task.Run(() =>
			{ //Taskを使ってクライアント待ち
				while (_execution)
				{
					//複数作ることもできるが、今回はwhileで1つずつ処理する
					using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1))
					{
						// クライアントの接続待ち
						pipeServer.WaitForConnection();

						StreamString ssSv = new StreamString(pipeServer);

						while (true)
						{ //データがなくなるまで                       
							string read = ssSv.ReadString(); //クライアントの引数を受信 
							if (string.IsNullOrEmpty(read))
							{
								break;
							}
							else
							{
								_Result = read;
								_execution = false;
							}
							if (!_execution)
								break; //起動停止？
						}
						ssSv = null;
					}
				}
			});
		}
		// ******************************************************************************
		public static Task PipeClient(string pipeName, string js)
		{
			return Task.Run(() =>
			{ //Taskを使ってサーバに送信waitで処理が終わるまで待つ
				using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation))
				{
					StreamString ssCl;
					string writeData;
					pipeClient.Connect();

					ssCl = new StreamString(pipeClient);
					writeData = js; //送信する引数
					ssCl.WriteString(writeData);
					ssCl = null;
				}
			});
		}
	}
	public class StreamString
	{
		private System.IO.Stream ioStream;
		private System.Text.UnicodeEncoding streamEncoding;
		public StreamString(System.IO.Stream ioStream)
		{
			this.ioStream = ioStream;
			streamEncoding = new System.Text.UnicodeEncoding();
		}

		// ********************************************************************
		public string ReadString()
		{
			int len = 0;
			len = ioStream.ReadByte() * 256; //テキスト長
			len += ioStream.ReadByte(); //テキスト長余り
			if (len > 0)
			{ //テキストが格納されている
				byte[] inBuffer = new byte[len];
				ioStream.Read(inBuffer, 0, len); //テキスト取得
				return streamEncoding.GetString(inBuffer);
			}
			else //テキストなし
				return "";
		}
		// ********************************************************************
		public int WriteString(string outString)
		{
			if (string.IsNullOrEmpty(outString))
				return 0;
			byte[] outBuffer = streamEncoding.GetBytes(outString);
			int len = outBuffer.Length; //テキストの長さ
			if (len > UInt16.MaxValue)
				len = (int)UInt16.MaxValue; //65535文字
			ioStream.WriteByte((byte)(len / 256)); //テキスト長
			ioStream.WriteByte((byte)(len & 255)); //テキスト長余り
			ioStream.Write(outBuffer, 0, len); //テキストを格納
			ioStream.Flush();
			return outBuffer.Length + 2; //テキスト＋２(テキスト長)
		}
	}
}
/*
 　　// 使用サンプル
	internal class Program
	{
		public static string CallExeName = "SkeltonWinForm";
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
#pragma warning restore CS8601 // Null 参照代入の可能性があります。
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
