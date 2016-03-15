using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using vandango.Systems.Swan.Microcontroller;

namespace vandango.Systems.Swan
{
	class Program
	{
		[DllImport("kernel32.dll", ExactSpelling = true)]

		private static extern IntPtr GetConsoleWindow();
		private static IntPtr ThisConsole = GetConsoleWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		private const int HIDE = 0;
		private const int MAXIMIZE = 3;
		private const int MINIMIZE = 6;
		private const int RESTORE = 9;

		private static List<string> arguments;

		[STAThread]
		static void Main(string[] args)
		{
			arguments = new List<string>(args);

			// help screen
			if (arguments.Contains("-?") || arguments.Contains("-h"))
			{
				// -v -a 108:00 04:00 00:10 -f
				Console.WriteLine("Possible start options:");
				Console.WriteLine();
				Console.WriteLine("   -? = This help screen");
				Console.WriteLine("   -h = This help screen");
				Console.WriteLine("   -v = Disable verbose mode");
				Console.WriteLine("   -f = Fullscreen");
				Console.WriteLine("   -t = Set timer values. Parameters will be set in this order:");
				Console.WriteLine("        1) Runtime in time format (eg. 108:00)");
				Console.WriteLine("        2) Attention time in time format (eg. 04:00)");
				Console.WriteLine("        3) Warning time time format (eg. 00:10)");

				Console.Read();
				return;
			}

			// init console
			Console.CancelKeyPress += Console_CancelKeyPress;
			Console.Clear();

			// set fullscreen
			bool fullscreen = (arguments.Count > 0 && arguments.Contains("-f"));
			if (fullscreen)
			{
				Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
				ShowWindow(ThisConsole, MAXIMIZE);
			}

			// start verbose (silent)
			bool silent = (arguments.Count <= 0 || !arguments.Contains("-v"));

			// set timer values
			TimeSpan? startTime = null;
			TimeSpan? attentionTime = null;
			TimeSpan? warningTime = null;

			// -t 108:00 04:00 00:10
			if (arguments.Any(a => a.StartsWith("-t")))
			{
				bool readTimeParameters = false;
				var times = new List<string>();

				arguments.ForEach(arg =>
				{
					// stop reading time parameters
					if(arg.StartsWith("-f")
						|| arg.StartsWith("-v"))
					{
						readTimeParameters = false;
					}

					// read time parameters
					if (readTimeParameters
						&& arg != "-t")
					{
						times.Add(arg);
					}

					// find time initializer
					if (arg.StartsWith("-t"))
					{
						readTimeParameters = true;
					}
				});

				int index = 1;
				int value;
				times.ForEach(arg =>
				{
					value = PenningElementDispatcherMicrocontroller.ParseStringToTimeInSeconds(arg);

					switch (index)
					{
						case 1:
							startTime = PenningElementDispatcherMicrocontroller.GetTimeSpan(value);

							if (!silent)
							{
								Console.WriteLine($">: StartTime initialized with {arg} ({value} = {PenningElementDispatcherMicrocontroller.FormatTimeOfSeconds(value, "mmm:ss")})");
                            }
							break;

						case 2:
							attentionTime = PenningElementDispatcherMicrocontroller.GetTimeSpan(value);

							if (!silent)
							{
								Console.WriteLine($">: AttentionTime initialized with {arg} ({value} = {PenningElementDispatcherMicrocontroller.FormatTimeOfSeconds(value, "mmm:ss")})");
							}
							break;

						case 3:
							warningTime = PenningElementDispatcherMicrocontroller.GetTimeSpan(value);

							if (!silent)
							{
								Console.WriteLine($">: WarningTime initialized with {arg} ({value} = {PenningElementDispatcherMicrocontroller.FormatTimeOfSeconds(value, "mmm:ss")})");
							}
							break;
					}

					index++;
				});
			}

			// run
			using (var dispatcher = new PenningElementDispatcherMicrocontroller(silent, startTime, attentionTime, warningTime))
			{
				dispatcher.StartDispatchCalculation();
				dispatcher.StandardInput();
				string parameters = Console.ReadLine();
				while(true)
				{
					var handleResult = dispatcher.HandleWorldDispatch(parameters);
					if (handleResult)
					{
						dispatcher.RestartDispatchCalculation();
					}

					if (!silent)
					{
						Thread.Sleep(1500);
					}

					Console.Clear();
					dispatcher.StandardInput();
					parameters = Console.ReadLine();
				}
			}
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Thread.Sleep(1000);
			Console.Clear();

			Console.Write(">: Stop <Microcontroller->WorldDispatchTimer>");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(" OK\n");
			Thread.Sleep(200);

			Console.Write(">: Stop <Microcontroller->PenningElementDispatcherMicrocontroller>");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(" OK\n");
			Thread.Sleep(200);

			Console.WriteLine(">: The world ends now!");
			Thread.Sleep(1000);
			Console.WriteLine(">: Good bye!");
			Thread.Sleep(1000);

			Console.Write(">: .");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Thread.Sleep(200);
			Console.Write(".");
			Console.Write("\n");
			Thread.Sleep(1000);

			Console.WriteLine(">: BOOOOM!");
			Thread.Sleep(1000);

			Console.WriteLine(">: Ok, just a joke. Only the program stops.");
			Thread.Sleep(1000);
		}
	}
}
