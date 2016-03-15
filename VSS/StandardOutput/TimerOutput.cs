using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vandango.Systems.Swan.Microcontroller;

namespace vandango.Systems.Swan.StandardOutput
{
	public partial class TimerOutput : Form
	{
		public TimerOutput()
		{
			InitializeComponent();

			this.Location = new Point(
				Screen.PrimaryScreen.WorkingArea.Right - this.Width,
				Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
				);
		}

		public string TimeField
		{
			get { return label1.Text; }
			set
			{
				label1.Invoke(() =>
				{
					label1.Text = value;
				});
			}
		}

		public bool IsDefault
		{
			set
			{
				label1.Invoke(() =>
				{
					label1.ForeColor = Color.White;
				});
			}
		}

		public bool IsAttention
		{
			set
			{
				label1.Invoke(() =>
				{
					label1.ForeColor = value ? Color.Orange : Color.White;
				});
			}
		}

		public bool IsWarning
		{
			set {
				label1.Invoke(() =>
				{
					label1.ForeColor = value ? Color.Red : Color.White;
				});
			}
		}
	}
}
