using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace WindowsApplication1 {

	public partial class Form1 : Form {
		private enum SW {
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_NORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_MAX = 10
		}

		private const int MYKEYID = 0;
		
		public IntPtr TargetConsoleWindow = (IntPtr)0;

		[DllImport("user32.dll")]
		private static extern int SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		public Form1() {
		}

		public bool RegisterHotkey(){
			if(!RegisterHotKey(this.Handle, MYKEYID, MOD_CONTROL, Keys.Oemtilde)){
				return false;
			} else {
				this.FormClosing += Form1_FormClosing;
			}
			return true;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			UnregisterHotKey(this.Handle, MYKEYID);
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(false);
		}

		protected override void WndProc(ref Message m) {
			if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == MYKEYID) {

				if (GetForegroundWindow() == TargetConsoleWindow){
					// How about AnimateWindow https://msdn.microsoft.com/en-us/library/windows/desktop/ms632669(v=vs.85).aspx
					ShowWindow(TargetConsoleWindow, (int)SW.SW_MINIMIZE);
				} else {
					ShowWindow(TargetConsoleWindow, (int)SW.SW_RESTORE);
					SetForegroundWindow(TargetConsoleWindow);
				}
			}
			base.WndProc(ref m);
		}

		private const int WM_HOTKEY = 0x312;
		private const int MOD_ALT = 1;
		private const int MOD_CONTROL = 2;
		private const int MOD_SHIFT = 4;
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifier, Keys vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}