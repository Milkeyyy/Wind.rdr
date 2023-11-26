/* 
	参考: https://mocotan.hatenablog.com/entry/2017/10/13/123957
*/

using Produire; // utopiat.Host.dllを参照すること
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Wind.rdr
{
	public class Wind : IProduireStaticClass // これを実装しないとプロデルから利用できない
	{
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
		[自分("で")]
		public プロセスウィンドウ[] ウィンドウ一覧取得する()
		{
			return GetWindowList();
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
	}
}
