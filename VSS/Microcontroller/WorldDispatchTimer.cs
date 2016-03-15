using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using vandango.Systems.Swan.StandardOutput;
using Timer = System.Threading.Timer;

namespace vandango.Systems.Swan.Microcontroller
{
	internal class WorldDispatchTimer
	{
		/// <summary>
		/// Timer handler and callback handler
		/// </summary>
		private Timer _callbackTimer;
		private readonly TimerCallback _callback;

		/// <summary>
		/// Dispatch time
		/// </summary>
		private TimeSpan DispatchTime { get; set; }

		/// <summary>
		/// Declare standard output
		/// </summary>
		private TimerOutput Output { get; set; }

		/// <summary>
		/// Offrers a value indicating if the standard input is open or not
		/// </summary>
		public bool StandardInputIsOpen => DispatchTime <= AttentionTime && DispatchTime > EndTime;

		/// <summary>
		/// Defaults
		/// </summary>
		private static TimeSpan StartTime = TimeSpan.FromSeconds(6480);	// 6480 Sekunden / 108 Minuten
		private static TimeSpan AttentionTime = TimeSpan.FromSeconds(240);	// 240 Sekunden / 4 Minuten
		private static TimeSpan WarningTime = TimeSpan.FromSeconds(10);	// 10 Sekunden
		private static TimeSpan EndTime = new TimeSpan();					// 0 Sekunden

		public WorldDispatchTimer(TimeSpan? startTime = null, TimeSpan? attentionTime = null, TimeSpan? warningTime = null)
		{
			if (startTime.HasValue)
			{
				StartTime = startTime.Value;
			}

			if (attentionTime.HasValue)
			{
				AttentionTime = attentionTime.Value;
			}

			if (warningTime.HasValue)
			{
				WarningTime = warningTime.Value;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Output = new TimerOutput();
			//Application.Run(_output);

			new System.Threading.Thread(() =>
			{
				Application.Run(Output);
			}).Start();

			//Console.WriteLine(">: Engrave <WorldDispatchTimer>");
			this._callback = TimerElapsed;
			
			DispatchTime = StartTime;
		}

		/// <summary>
		/// Start timer
		/// </summary>
		public void Start()
		{
			//Console.WriteLine(">: Revive <WorldDispatchTimer>");
			//Console.Write("\r{0} >:", TimeToString(_dispatchTime));
			Output.IsDefault = true;
			Output.TimeField = TimeToString(DispatchTime);
			this._callbackTimer = new Timer(
				this._callback,
				null,
				1000,
				1000
				);
		}

		/// <summary>
		/// Stop the work of the worker
		/// </summary>
		public void Stop()
		{
			this._callbackTimer?.Dispose();
		}

		/// <summary>
		/// Restarts the work of the worker
		/// </summary>
		public void Restart()
		{
			this.Stop();
			// TODO: do some fancy stuff...
			DispatchTime = StartTime;
			this.Start();
		}

		/// <summary>
		/// Run through all periods
		/// </summary>
		/// <param name="obj"></param>
		private void TimerElapsed(object obj)
		{
			var timeBefore = DispatchTime;
			DispatchTime = DispatchTime.Subtract(TimeSpan.FromSeconds(1));

			if (DispatchTime > AttentionTime)
			{
				if (Math.Abs(Math.Ceiling(timeBefore.TotalMinutes) - Math.Ceiling(DispatchTime.TotalMinutes)) > 0)
				{
					//	Console.Write(
					//		"\r{0}:00",
					//		Math.Ceiling(_dispatchTime.TotalMinutes)
					//			.ToString(CultureInfo.InvariantCulture)
					//			.PadLeft(3, '0')
					//		);
					Output.IsDefault = true;
					Output.TimeField = string.Format(
						"{0}:00",
						Math.Ceiling(DispatchTime.TotalMinutes)
							.ToString(CultureInfo.InvariantCulture)
							.PadLeft(3, '0')
						);
				}

				//Console.Write("\r{0} >:", TimeToString(_dispatchTime));
				//Output.IsDefault = true;
				//Output.TimeField = TimeToString(DispatchTime);
			}

			if (DispatchTime <= AttentionTime && DispatchTime > WarningTime)
			{
				// TODO: do some fancy stuff...
				//Console.Write("\r{0} >:", TimeToString(_dispatchTime));
				Output.IsAttention = true;
				Output.TimeField = TimeToString(DispatchTime);
			}

			if (DispatchTime <= WarningTime && DispatchTime > EndTime)
			{
				// sound
				// TODO: do some fancy stuff...
				//Console.Write("\r{0} !Beep! >:", TimeToString(_dispatchTime));
				Output.IsWarning = true;
				Output.TimeField = TimeToString(DispatchTime);
			}

			if (DispatchTime <= EndTime)
			{
				_callbackTimer.Dispose();
				_callbackTimer = null;

				// sound
				// TODO: do some fancy stuff...
				Output.IsWarning = true;
				Output.TimeField = TimeToString(EndTime);

				Console.Clear();
				Console.WriteLine(">: World dispatching stopped!");
				Thread.Sleep(1000);
				Console.WriteLine(">: Critical mass reached...");
				Thread.Sleep(1000);
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

				Output.TimeField = "BOOOM!";
				Thread.Sleep(1000);

				Console.WriteLine(">: Ok, just a joke. Only the program stops.");
				Thread.Sleep(1000);
				Environment.Exit(0);
			}
		}

		private string TimeToString(TimeSpan time)
		{
			return string.Concat(
				Math.Floor(time.TotalMinutes).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'),
				":",
				time.Seconds.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
				);
		}
	}
}
