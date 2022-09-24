using BRY;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Drawing.Text;

namespace m2p
{
	public enum Mode
	{
		Mm,
		Dpi,
		Pixel,
		Mult
	}



	public partial class MainForm : Form
	{
		private bool _refFlag = false;
		private Mode m_Mode = Mode.Mm;
		public static bool _execution = true;
		// ********************************************************************
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		// ********************************************************************
		public MainForm()
		{
			/*
			PrivateFontCollection pfc =new PrivateFontCollection();
			byte[] fontBuf = Properties.Resources.SourceHanCodeJP_0;

			unsafe
			{
				fixed (byte* pFontBuf = fontBuf)
				{
					//PrivateFontCollectionにフォントを追加する
					pfc.AddMemoryFont((IntPtr)pFontBuf, fontBuf.Length);
				}
			}
			//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
			int c = pfc.Families.Length;
			Font f10 = new Font(pfc.Families[3], 10);
			Font f12 = new Font(pfc.Families[3], 12);
			Font f14 = new Font(pfc.Families[3], 14);
			//this.UseCompatibleTextRendering = true;
			this.Font = f12;
			*/
			InitializeComponent();
			/*
			if (this.Controls.Count > 0)
			{
				for(int i=0; i< this.Controls.Count; i++)
				{
					try
					{
						if(this.Controls[i] is Button)
						{
							((Button)this.Controls[i]).UseCompatibleTextRendering = true;
							this.Controls[i].Font = f14;
						}
						else if (this.Controls[i] is RadioButton)
						{
							((RadioButton)this.Controls[i]).UseCompatibleTextRendering = true;
							this.Controls[i].Font = f10;
						}
					}
					catch
					{

					}
				}
			}
			*/
			mmToolStripMenuItem.Tag = (int)Mode.Mm;
			dpiToolStripMenuItem.Tag = (int)Mode.Dpi;
			pixelToolStripMenuItem.Tag = (int)Mode.Pixel;
			multToolStripMenuItem.Tag = (int)Mode.Mult;
			rbM.Tag = (int)Mode.Mm;
			rbD.Tag = (int)Mode.Dpi;
			rbP.Tag = (int)Mode.Pixel;
			rbMul.Tag = (int)Mode.Mult;
			tbMm.Tag = (int)Mode.Mm;
			tbDpi.Tag = (int)Mode.Dpi;
			tbPixel.Tag = (int)Mode.Pixel;
			tbMul.Tag = (int)Mode.Mult;
			SetMode(Mode.Mm);
		}

		// ********************************************************************
		private void Form1_Load(object sender, EventArgs e)
		{
			PrefFile pf = new PrefFile();
			this.Text = pf.AppName;
			if (pf.Load() == true)
			{
				bool ok = false;
				Point p = pf.GetPoint("Loc", out ok);
				if (ok)
				{
					this.Location = p;
				}
				Rectangle r = this.Bounds;
				if (PrefFile.ScreenIn(r) == false)
				{
					ToCenter();
				}
			}
			//
			Command(Environment.GetCommandLineArgs().Skip(1).ToArray(), PIPECALL.StartupExec);
			//this.Text = nameof(MainForm.Parent) + "/aa";
		}
		// ********************************************************************
		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			PrefFile pf = new PrefFile();
			pf.SetPoint("Loc", this.Location);
			pf.Save();
		}
		// ********************************************************************
		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		// ********************************************************************
		public void ToCenter()
		{
			Rectangle rct = Screen.PrimaryScreen.Bounds;
			Point p = new Point((rct.Width - this.Width) / 2, (rct.Height - this.Height) / 2);
			this.Location = p;
			ForegroundWindow();
		}
		// ********************************************************************
		public void SetMode(Mode m)
		{

			//if (m_Mode == m) return;
			Mode nm = m_Mode;
			m_Mode = m;
			switch (nm)
			{
				case Mode.Mm:
					mmToolStripMenuItem.Checked = false;
					break;
				case Mode.Dpi:
					dpiToolStripMenuItem.Checked = false;
					btnClear.Enabled = true;
					break;
				case Mode.Pixel:
					pixelToolStripMenuItem.Checked = false;
					break;
				case Mode.Mult:
					multToolStripMenuItem.Checked = false;
					break;
			}
			switch (m_Mode)
			{
				case Mode.Mm:
					rbM.Checked = true;
					mmToolStripMenuItem.Checked = true;
					try { tbMm.Focus(); } catch { }
					break;
				case Mode.Dpi:
					rbD.Checked = true;
					dpiToolStripMenuItem.Checked = true;
					btnClear.Enabled = false;
					try { tbDpi.Focus(); } catch { }
					break;
				case Mode.Pixel:
					rbP.Checked = true;
					pixelToolStripMenuItem.Checked = true;
					try { tbPixel.Focus(); } catch { }
					break;
				case Mode.Mult:
					rbMul.Checked = true;
					multToolStripMenuItem.Checked = true;
					try { tbMul.Focus(); } catch { }
					break;
			}
		}
		private void mmToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem mi = (ToolStripMenuItem)sender;
			SetMode((Mode)mi.Tag);
		}
		private void rbM_Click(object sender, EventArgs e)
		{
			RadioButton mi = (RadioButton)sender;
			SetMode((Mode)mi.Tag);
		}
		private void tbMm_Enter(object sender, EventArgs e)
		{
			TextBox mi = (TextBox)sender;
			SetMode((Mode)mi.Tag);
		}
		// ********************************************************************
		public void Clear()
		{
			switch(m_Mode)
			{
				case Mode.Mm:
					tbMm.Text = "";
					break;
				case Mode.Pixel:
					tbPixel.Text = "";
					break;
				case Mode.Mult:
					tbMul.Text = "1";
					break;
			}
		}
		// ********************************************************************
		private void btnClear_Click(object sender, EventArgs e)
		{
			Clear();
		}
		// ********************************************************************
		private void BackSpace(TextBox tb)
		{
			if (tb.Text != "")
			{
				tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
			}
		}
		// ********************************************************************
		public void BackSpace()
		{
			switch (m_Mode)
			{
				case Mode.Mm:
					BackSpace(tbMm);
					break;
				case Mode.Pixel:
					BackSpace(tbPixel);
					break;
				case Mode.Dpi:
					BackSpace(tbDpi);
					break;
				case Mode.Mult:
					BackSpace(tbMul);
					break;
			}
		}
		// ********************************************************************
		private void btnBS_Click(object sender, EventArgs e)
		{
			BackSpace();
		}
		// ********************************************************************
		private void AddStr(TextBox tb, string s)
		{
			string str = tb.Text;
			int ss = tb.SelectionStart;
			int sl = tb.SelectionLength;
			if(sl==0)
			{
				str = str.Substring(0, ss) + s + str.Substring(ss);
			}
			else
			{
				str = str.Substring(0, ss) + s + str.Substring(ss+sl);
			}
			tb.Text = str;
			tb.Select(ss + s.Length, 0);

		}
		// ********************************************************************
		private void AddStr(string s)
		{
			switch (m_Mode)
			{
				case Mode.Mm:
					AddStr(tbMm,s);
					break;
				case Mode.Pixel:
					AddStr(tbPixel, s);
					break;
				case Mode.Dpi:
					AddStr(tbDpi, s);
					break;
				case Mode.Mult:
					AddStr(tbMul, s);
					break;
			}
		}
		// ********************************************************************
		private void SetStr(string s)
		{
			switch (m_Mode)
			{
				case Mode.Mm:
					tbMm.Text = s;
					break;
				case Mode.Pixel:
					tbPixel.Text = s;
					break;
				case Mode.Dpi:
					tbDpi.Text = s;
					break;
				case Mode.Mult:
					tbMul.Text = s;
					break;
			}
		}
		// ********************************************************************
		private void button1_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			AddStr(btn.Text);
		}
		// ********************************************************************
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			Keys k = e.KeyData;
			this.Text = k.ToString();
			//if((k>=Keys.NumPad0)&& (k <= Keys.NumPad9)

		}
		// ********************************************************************
		public double GetValue(TextBox tb)
		{
			double ret = 0;
			double b = 0;
			try
			{
				if (Double.TryParse(tb.Text, out b))
				{
					ret = b;
				}
			}
			catch
			{

			}
			return ret;
		}
		// ********************************************************************
		private double FloorDouble(double d)
		{
			return (double)((int)(d * 10000 + 0.5)) / 10000;
		}
		// ********************************************************************
		public void SetMm(string s = "")
		{
			if (_refFlag) return;
			_refFlag = true;
			bool err = false;
			if (s != "") tbMm.Text = s;
			double m = GetValue(tbMm);
			if (m <= 0)
			{
				err = true;
			}
			double d = GetValue(tbDpi);
			if (d < 72) err = true;

			string ps = "";
			if (err==false)
			{
				double p = FloorDouble(m * d / 25.4);
				ps = p.ToString();
			}
			tbPixel.Text = ps;
			_refFlag = false;
		}
		// ********************************************************************
		public void SetDpi(string s = "")
		{
			if (_refFlag) return;
			_refFlag = true;
			bool err = false;
			if (s != "") tbDpi.Text = s;
			double m = GetValue(tbMm);
			if (m <= 0) err = true;
			double d = GetValue(tbDpi);
			if (d < 72)
			{
				err = true;
			}

			string ps = "";
			if (err == false)
			{
				double p = FloorDouble(m * d / 25.4);
				ps = p.ToString();
			}
			tbPixel.Text = ps;
			_refFlag = false;
		}
		// ********************************************************************
		public void SetPixel(string s = "")
		{
			if (_refFlag) return;
			_refFlag = true;
			bool err = false;
			if (s != "") tbPixel.Text = s;
			double p = GetValue(tbPixel);
			if (p <= 0) err = true;
			double d = GetValue(tbDpi);
			if (d <= 0) err = true;

			string ps = "";
			if (err == false)
			{
				double m = FloorDouble(p * 25.4 / d);
				ps = m.ToString();
			}
			tbMm.Text = ps;
			_refFlag = false;
		}
		// ********************************************************************
		private void tbMm_TextChanged(object sender, EventArgs e)
		{
			if (_refFlag) return;
			SetMm();
		}
		private void tbDpi_TextChanged(object sender, EventArgs e)
		{
			if (_refFlag) return;
			SetDpi();

		}
		private void tbPixel_TextChanged(object sender, EventArgs e)
		{
			if (_refFlag) return;
			SetPixel();
		}
		// ********************************************************************
		public void MultiExec(TextBox tb)
		{
			double b = GetValue(tb);
			if (b <= 0) return;
			double m = GetValue(tbMul);
			if((m<=0)||(m==1)) return;
			b = FloorDouble(b * m);
			tb.Text = b.ToString();
			tbMul.Text = "1";
		}
		// ********************************************************************
		public void MultiExecMm()
		{
			MultiExec(tbMm);
		}
		// ********************************************************************
		public void MultiExecPixel()
		{
			MultiExec(tbPixel);
		}
		private void btnMultMm_Click(object sender, EventArgs e)
		{
			MultiExecMm();
		}
		private void button13_Click(object sender, EventArgs e)
		{
			MultiExecPixel();
		}
		private void multMToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MultiExecMm();
		}
		private void multPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MultiExecPixel();
		}
		// ********************************************************************
		public void CopyExec(TextBox tb)
		{
			Clipboard.SetText(tb.Text);
		}
		public void CopyExecMm(){CopyExec(tbMm); }
		public void CopyExecDpi() { CopyExec(tbDpi); }
		public void CopyExecPixel() { CopyExec(tbPixel); }
		// ********************************************************************
		public void ForegroundWindow()
		{
			SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
		}
		// ********************************************************************
		public void Command(string[] args, PIPECALL IsPipe = PIPECALL.StartupExec)
		{
			bool err = true;
			Args args1 = new Args(args);
			if (args1.OptionCount > 0)
			{
				for (int i = 0; i < args1.OptionCount; i++)
				{
					if (err == false) break;
					Param p = args1.Option(i);
					switch (p.OptionStr.ToLower())
					{
						case "tocenter":
						case "center":
							ToCenter();
							break;
						case "foregroundWindow":
						case "foreground":
							ForegroundWindow();
							break;
						case "exit":
						case "quit":
							if ((args1.ParamsCount == 1) && (IsPipe == PIPECALL.DoubleExec))
							{
								Application.Exit();
							}
							break;
						case "setmode":
						case "mode":
							int idx0 = p.Index + 1;
							if ((idx0 < args1.ParamsCount) && (args1.Params[idx0].IsOption==false))
							{
								string c0 = args1.Params[idx0].Arg.ToLower();
								switch(c0)
								{
									case "mm":
									case "m":
										SetMode(Mode.Mm);
										break;
									case "dpi":
									case "d":
										SetMode(Mode.Dpi);
										break;
									case "pixel":
									case "p":
										SetMode(Mode.Pixel);
										break;
									case "mult":
									case "multi":
									case "mu":
										SetMode(Mode.Mult);
										break;
								}
							}
							break;
						case "value":
						case "setvalue":
							int idx1 = p.Index + 1;
							if ((idx1 < args1.ParamsCount) && (args1.Params[idx1].IsOption == false))
							{
								SetStr(args1.Params[idx1].Arg);
							}
							break;
						case "copy":
						case "clip":
							int idx2 = p.Index + 1;
							if ((idx2 < args1.ParamsCount) && (args1.Params[idx2].IsOption == false))
							{
								string c2 = args1.Params[idx2].Arg.ToLower();
								switch (c2)
								{
									case "mm":
									case "m":
										CopyExecMm();
										break;
									case "dpi":
									case "d":
										CopyExecDpi();
										break;
									case "pixel":
									case "p":
										CopyExecPixel();
										break;
								}
							}
							break;

					}
				}
			}
		}
		// *******************************************************************************
		// *******************************************************************************
		static public void ArgumentPipeServer(string pipeName)
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
								break;

							//引数が受信できたら、Applicationに登録されているだろうForm1に引数を送る
							FormCollection apcl = Application.OpenForms;

							if (apcl.Count > 0)
							{
								PipeData pd = new PipeData(read);
								((MainForm)apcl[0]).Command(pd.GetArgs(), pd.GetPIPECALL()); //取得した引数を送る
							}

							if (!_execution)
								break; //起動停止？
						}
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
						ssSv = null;
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
					}
				}
			});
		}
		// ********************************************************************


		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cmb = (ComboBox)sender;
			if (cmb.SelectedIndex < 0) return;
			tbDpi.Text = cmb.SelectedItem.ToString();
			cmb.SelectedIndex = -1;
		}

	}

	// ********************************************************************
}