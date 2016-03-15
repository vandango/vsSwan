using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vandango.Systems.Swan.Microcontroller
{
	internal class PenningElementDispatcherMicrocontroller : IDisposable
	{
		private static readonly Dictionary<int, int> WorldParameter = new Dictionary<int, int>()
		{
			{ 0, 4 },
			{ 1, 8 },
			{ 2, 15 },
			{ 3, 16 },
			{ 4, 23 },
			{ 5, 42 }
		};

		/// <summary>
		/// Identifier to detect redundant calls
		/// </summary>
		private bool _disposed = false;

		private bool _silent = false;

		/// <summary>
		/// Creates a new instance of the timer
		/// </summary>
		private readonly WorldDispatchTimer _timer;
		private Thread _timerThread;

		/// <summary>
		/// Creates a new <see cref="PenningElementDispatcherMicrocontroller"/> application
		/// inside the System Swan micro controller environment.
		/// </summary>
		public PenningElementDispatcherMicrocontroller(bool silent = false, TimeSpan? startTime = null, TimeSpan? attentionTime = null, TimeSpan? warningTime = null)
		{
			_silent = silent;

			if (!_silent)
			{
				Console.WriteLine(">: Initializing <Microcontroller->PenningElementDispatcherMicrocontroller>");
				Console.WriteLine(">: Initializing <Microcontroller->WorldDispatchTimer>");
			}

			_timer = new WorldDispatchTimer(startTime, attentionTime, warningTime);
		}

		/// <summary>
		/// Start dispatch calculation
		/// </summary>
		public void StartDispatchCalculation()
		{
			if (!_silent)
			{
				Console.WriteLine(">: Start <Microcontroller->WorldDispatchTimer>");
			}
			_timerThread = new Thread(_timer.Start);
			_timerThread.Start();
		}

		/// <summary>
		/// Restart dispatch calculation
		/// </summary>
		public void RestartDispatchCalculation()
		{
			if (!_silent)
			{
				Console.WriteLine(">: Restart <Microcontroller->WorldDispatchTimer>");
			}
			_timer.Restart();
		}

		/// <summary>
		/// Shows the standard input handle
		/// </summary>
		public void StandardInput()
		{
			Console.Write(">: ");
		}

		/// <summary>
		/// Handle world dispatch
		/// </summary>
		/// <param name="worldParameters"></param>
		public bool HandleWorldDispatch(string worldParameters)
		{
			if (string.IsNullOrWhiteSpace(worldParameters))
			{
				return false;
			}

			if (!_timer.StandardInputIsOpen)
			{
				// standard input is actually not open
				if (!_silent)
				{
					Console.WriteLine(">: Input is actually not open!");
				}

				return false;
			}

			var parameters = worldParameters.Split(' ');

			if (!_silent)
			{
				Console.WriteLine(">: World parameters: {0}", worldParameters);
			}

			if (parameters.Length != WorldParameter.Count)
			{
				// incorrect parameter amount
				if (!_silent)
				{
					Console.WriteLine(">: Incorrect world parameter amount entered!");
				}
				
				return false;
			}
			else
			{
				if (WorldParameter.Any(pair => Convert.ToInt32(parameters[pair.Key]) != pair.Value))
				{
					if (!_silent)
					{
						Console.WriteLine(">: Incorrect world parameter entered!");
					}
					
					return false;
				}

				// correct parameter entered, restart timer
				if (!_silent)
				{
					Console.WriteLine(">: Correct world parameter entered!");
				}

				_timer.Stop();
				_timerThread.Abort();

				if (!_silent)
				{
					Console.WriteLine(">: Timer stopped!");
				}

				return true;
			}
		}

		/// <summary>
		/// Clear system swan cache
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Run clear system swan cache
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Dispose explicit instances here
					_timer.Stop();
				}

				_disposed = true;
			}
		}
		
		/*
			STATIC METHODS
		*/

		public static TimeSpan GetTimeSpan(int seconds)
		{
			return TimeSpan.FromSeconds(seconds);
		}

		public static string ConvertDateTimeFormatToTimeSpanFormat(string dateTimeFormatString)
		{
			string timeFormatDefault = "d\\.hh\\:mm\\:ss";

			if (dateTimeFormatString.IsNullOrTrimmedEmpty())
			{
				return timeFormatDefault;
			}

			//if (dateTimeFormatString == "HH:mm:ss")
			//{
			//	return "hh\\:mm\\:ss";
			//}

			//if (dateTimeFormatString == "HH:mm")
			//{
			//	return "hh\\:mm\\";
			//}

			return dateTimeFormatString
				.Replace("HH", "hh")
				.Replace(":", "\\:");
		}

		public static string FormatTimeOfMinutes(int timeInMinutes, string timeFormat = "HH:mm")
		{
			//double tmp = Convert.ToDouble(timeInMinutes) / 60;
			//string s = tmp.ToString();
			//string pre = "";

			//if (s.StartsWith("-"))
			//{
			//	pre = s.Substring(0, 1) + " ";
			//	s = s.Substring(1);
			//}

			//s = s.Substring(0, (s.IndexOf(",") > 0 ? s.IndexOf(",") : s.Length));
			//int h = Convert.ToInt32(s);
			//int m = Math.Abs(timeInMinutes) - (h * 60);

			//return pre + h.ToString().PadLeft(2, '0') + ":" + m.ToString().PadLeft(2, '0');
			
			var full = TimeSpan.FromMinutes(timeInMinutes).ToString(ConvertDateTimeFormatToTimeSpanFormat(timeFormat));
			return full.Substring(0, full.LastIndexOf(":", StringComparison.Ordinal));
		}

		public static string FormatTimeOfSeconds(int timeInSeconds, string timeFormat = "mm:ss")
		{
			if (timeFormat.Contains("mmm"))
			{
				var time = TimeSpan.FromSeconds(timeInSeconds);
				return $"{(int) time.TotalMinutes:00}:{time.Seconds:00}";
			}

			return TimeSpan.FromSeconds(timeInSeconds).ToString(ConvertDateTimeFormatToTimeSpanFormat(timeFormat));
		}

		/// <summary>
		/// Parse a time string (eg. 120, 0:30, 0,5, 0.5, 2:30:15) to a seconds value
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static int ParseStringToTimeInSeconds(string value)
		{
			int hour = 0;
			int min = 0;
			int sec = 0;

			if (value != null
			&& value.Trim() != "")
			{
				try
				{
					string[] parts;

					if (value.Contains(":"))
					{
						// 0:30 or 2:30:15
						parts = value.Split(':');

						if (parts.Length == 3)
						{
							// 2:30:15
							hour = parts[0].ToInt32();
							min = parts[1].ToInt32();
							sec = parts[2].ToInt32();
						}
						else
						{
							// 0:30
							min = parts[0].ToInt32();
							sec = parts[1].ToInt32();
						}
					}
					else if (value.Contains(","))
					{
						// 0,5
						parts = value.Split(',');
						min = parts[0].ToInt32();

						if (parts[1].Length > 1)
						{
							sec = parts[1].Substring(0, 2).PadRight(2, '0').ToInt32();
						}
						else
						{
							sec = parts[1].PadRight(2, '0').ToInt32();
						}

						sec = GetTimeValueByPercent(sec);
					}
					else if (value.Contains("."))
					{
						// 0.5
						parts = value.Split('.');
						min = parts[0].ToInt32();

						if (parts[1].Length > 1)
						{
							sec = parts[1].Substring(0, 2).PadRight(2, '0').ToInt32();
						}
						else
						{
							sec = parts[1].PadRight(2, '0').ToInt32();
						}

						sec = GetTimeValueByPercent(sec);
					}
					else
					{
						// 30
						sec = value.ToInt32();
					}
				}
				catch (Exception)
				{
					// ignored
				}
			}

			return (hour * 60 * 60) + (min * 60) + sec;
		}

		/// <summary>
		/// Parse a time string (eg. 120, 0:30, 0,5, 0.5) to a minutes value
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="singleNumberIsHour">A value that indicates if a single number is a hour.</param>
		/// <returns></returns>
		public static int ParseStringToTimeInMinutes(string value, bool singleNumberIsHour = false)
		{
			int hour = 0;
			int min = 0;
			string[] parts;

			if (value != null
			&& value.Trim() != "")
			{
				try
				{
					if (value.Contains(":"))
					{
						// 0:30
						parts = value.Split(new char[] { ':' });
						hour = parts[0].ToInt32();
						min = parts[1].ToInt32();
					}
					else if (value.Contains(","))
					{
						// 0,5
						parts = value.Split(new char[] { ',' });
						hour = parts[0].ToInt32();

						if (parts[1].Length > 1)
						{
							min = parts[1].Substring(0, 2).PadRight(2, '0').ToInt32();
						}
						else
						{
							min = parts[1].PadRight(2, '0').ToInt32();
						}

						min = GetTimeValueByPercent(min);
					}
					else if (value.Contains("."))
					{
						// 0.5
						parts = value.Split('.');
						hour = parts[0].ToInt32();

						if (parts[1].Length > 1)
						{
							min = parts[1].Substring(0, 2).PadRight(2, '0').ToInt32();
						}
						else
						{
							min = parts[1].PadRight(2, '0').ToInt32();
						}

						min = GetTimeValueByPercent(min);
					}
					else
					{
						// 30
						if (singleNumberIsHour)
						{
							if (value.Length == 1)
							{
								hour = value.ToInt32();
							}
							else
							{
								min = value.ToInt32();
							}
						}
						else
						{
							min = value.ToInt32();
						}
					}
				}
				catch (Exception)
				{
					// ignored
				}
			}

			return (hour * 60) + min;
		}

		public static int GetTimeValueByPercent(double percent)
		{
			return Convert.ToInt32(
				Math.Round((percent / Convert.ToDouble(100) * Convert.ToDouble(60)))
			);
		}
	}

	internal static class Extensions
	{
		public static bool IsInt(this string number)
		{
			if (number == null)
			{
				throw new ArgumentNullException(nameof(number));
			}

			if (number.StartsWith("-"))
			{
				number = number.Substring(1);
			}

			return number.All(Char.IsDigit);
		}

		public static bool IsNullOrTrimmedEmpty(this string instance)
		{
			if (instance != null)
			{
				for (int i = 0; i < instance.Length; i++)
				{
					if (instance[i] != ' ')
					{
						return false;
					}
				}
			}

			return true;
		}

		public static int ToInt32(this string instance)
		{
			if (instance.IsNullOrTrimmedEmpty())
			{
				return 0;
			}

			int ret;

			if (int.TryParse(instance, out ret))
			{
				return ret;
			}
			else
			{
				if (instance.Contains("."))
				{
					instance = instance.Substring(0, instance.IndexOf(".", StringComparison.Ordinal));
				}

				if (instance.Contains(","))
				{
					instance = instance.Substring(0, instance.IndexOf(",", StringComparison.Ordinal));
				}

				if (int.TryParse(instance, out ret))
				{
					return ret;
				}

				return default(int);
			}
		}
	}
}
