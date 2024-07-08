using Produire;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Wind.rdr
{
	public class Wind : IProduireStaticClass
	{
		/* 
			参考: https://mocotan.hatenablog.com/entry/2017/10/13/123957
		*/

		private delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lParam);
		
		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("USER32.DLL")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		public class Window
		{
			public string Title { get; set; }
			public IntPtr Handle { get; set; }
			public int ProcessId { get; set; }

			public Window()
			{
				Title = "";
				Handle = IntPtr.Zero;
				ProcessId = 0;
			}
		}
		
		private static List<プロセスウィンドウ> WindowList { get; set; }

		[除外]
		public static プロセスウィンドウ[] GetWindowList()
		{
			WindowList = new List<プロセスウィンドウ>();
			// 表示されているウィンドウを取得する
			EnumWindows(EnumerateWindows, IntPtr.Zero);
			// 配列に変換して返す
			return WindowList.ToArray();
		}

		private static bool EnumerateWindows(IntPtr hWnd, IntPtr lParam)
		{
			if (IsWindowVisible(hWnd) == false) return true;

			int windowTextLength = GetWindowTextLength(hWnd);
			if (windowTextLength == 0) return true;

			// ウィンドウのタイトル
			var windowTitle = new StringBuilder(windowTextLength + 1);
			GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

			// プロセスのID
			int pid;
			GetWindowThreadProcessId(hWnd, out pid);

			WindowList.Add(new プロセスウィンドウ()
				{
					title = windowTitle.ToString(),
					handle = hWnd,
					processId = pid
				}
			);
			
			return true;
		}

		// 手順
		[自分で, 手順名("ウィンドウ一覧を", "取得する")]
		public プロセスウィンドウ[] ウィンドウ一覧を取得する()
		{
			return GetWindowList();
		}

		[手順名("ウィンドウを", "選択する")]
		public bool ウィンドウを選択する([という] int ウィンドウハンドル)
		{
			IntPtr handle = new IntPtr(ウィンドウハンドル);
			return SetForegroundWindow(handle);
		}

		[手順名("ウィンドウへ", "フォーカスする")]
		public bool ウィンドウへフォーカスする([という] int ウィンドウハンドル)
		{
			return ウィンドウを選択する(ウィンドウハンドル);
		}

		public class プロセスウィンドウ : IProduireClass
		{
			public string title;
			public IntPtr handle;
			public int processId;

			public プロセスウィンドウ()
			{
				title = "";
				handle = IntPtr.Zero;
				processId = 0;
			}

			public string タイトル { get { return title; } }
			public int ハンドル { get { return handle.ToInt32(); } }
			public int プロセスID { get { return processId; } }
		}

		// ウィンドウの属性
		public enum DWMWINDOWATTRIBUTE
		{
			DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
			DWMWA_WINDOW_CORNER_PREFERENCE = 33,
			DWMWA_MICA_EFFECT = 1029,
			DWMWA_LAST
		}

		public enum DWM_WINDOW_CORNER_PREFERENCE
		{
			DWMWCP_DEFAULT = 0,
			DWMWCP_DONOTROUND = 1,
			DWMWCP_ROUND = 2,
			DWMWCP_ROUNDSMALL = 3
		}

		[DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		internal static extern int DwmSetWindowAttribute(
			IntPtr hwnd,
			DWMWINDOWATTRIBUTE attribute,
			ref int pvAttribute,
			uint cbAttribute
		);

		public void ダークタイトルバー適用する([へ] IntPtr ウィンドウハンドル)
		{
			int value = 1;
			_ = DwmSetWindowAttribute(ウィンドウハンドル, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, (uint)Marshal.SizeOf(typeof(int)));
		}
	}
}
