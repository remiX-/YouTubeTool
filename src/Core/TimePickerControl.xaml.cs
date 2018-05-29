using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YouTubeTool.Core
{
	public partial class TimePickerControl : UserControl
	{
		private bool _isManuallyMutating;

		public int Seconds
		{
			get { return (int)GetValue(SecondsProperty); }
			set { SetValue(SecondsProperty, value); }
		}

		public static readonly DependencyProperty SecondsProperty =
			DependencyProperty.Register(nameof(Seconds), typeof(int), typeof(TimePickerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

		public int Minutes
		{
			get { return (int)GetValue(MinutesProperty); }
			set { SetValue(MinutesProperty, value); }
		}

		public static readonly DependencyProperty MinutesProperty =
			DependencyProperty.Register(nameof(Minutes), typeof(int), typeof(TimePickerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

		public int Hours
		{
			get { return (int)GetValue(HoursProperty); }
			set { SetValue(HoursProperty, value); }
		}

		public static readonly DependencyProperty HoursProperty =
			DependencyProperty.Register(nameof(Hours), typeof(int), typeof(TimePickerControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

		public TimeSpan Value
		{
			get { return (TimeSpan)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(TimeSpan), typeof(TimePickerControl), new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

		public TimePickerControl()
		{
			InitializeComponent();
		}

		private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var control = obj as TimePickerControl;
			if (control._isManuallyMutating) return;

			if (control.Seconds == 60)
			{
				control.Seconds = 0;
				control.Minutes++;
			}
			else if (control.Seconds == -1)
			{
				control.Seconds = 59;
				control.Minutes--;
			}

			if (control.Minutes == 60)
			{
				control.Minutes = 0;
				control.Hours++;
			}
			else if (control.Minutes == -1)
			{
				control.Minutes = 59;
				control.Hours--;
			}

			if (control.Hours == 24 || control.Hours == -1)
			{
				control.Hours = 0;
			}

			control._isManuallyMutating = true;
			control.SetCurrentValue(ValueProperty, new TimeSpan(control.Hours, control.Minutes, control.Seconds));
			control._isManuallyMutating = false;
		}

		private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var control = obj as TimePickerControl;
			if (control._isManuallyMutating) return;

			control._isManuallyMutating = true;
			control.SetCurrentValue(HoursProperty, control.Value.Hours);
			control.SetCurrentValue(MinutesProperty, control.Value.Minutes);
			control.SetCurrentValue(SecondsProperty, control.Value.Seconds);
			control._isManuallyMutating = false;
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			bool up = e.Delta > 0;

			switch (((TextBlock)sender).Name)
			{
				case "hour":
					Hours = Hours + (up ? 1 : -1);
					break;
				case "minute":
					Minutes = Minutes + (up ? 1 : -1);
					break;
				case "second":
					Seconds = Seconds + (up ? 1 : -1);
					break;
				default:
					throw new Exception("Not handled");
			}
		}
	}
}