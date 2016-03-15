using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vandango.Systems.Swan.Microcontroller
{
	public static class ControlExtensions
	{
		public static void Invoke(this Control control, Action action)
		{
			try
			{
				if (control != null && !control.IsDisposed)
				{
					control.Invoke(action);
				}
			}
			catch
			{
				// ignored
			}
		}
	}
}
