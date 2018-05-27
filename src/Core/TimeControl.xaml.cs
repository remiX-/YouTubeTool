using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YouTubeTool.Core
{
	public partial class TimeControl : UserControl
	{
		public int Seconds
		{
			get { return (int)GetValue(SecondsProperty); }
			set { SetValue(SecondsProperty, value); }
		}

		public static readonly DependencyProperty SecondsProperty = DependencyProperty.Register("Seconds", typeof(int), typeof(TimeControl),
		new UIPropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

		public int Minutes
		{
			get { return (int)GetValue(MinutesProperty); }
			set { SetValue(MinutesProperty, value); }
		}

		public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register("Minutes", typeof(int), typeof(TimeControl),
		new UIPropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

		public int Hours
		{
			get { return (int)GetValue(HoursProperty); }
			set { SetValue(HoursProperty, value); }
		}

		public static readonly DependencyProperty HoursProperty = DependencyProperty.Register("Hours", typeof(int), typeof(TimeControl),
		new UIPropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

		public TimeSpan Value
		{
			get { return (TimeSpan)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeControl),
		new UIPropertyMetadata(DateTime.Now.TimeOfDay, new PropertyChangedCallback(OnValueChanged)));

		public TimeControl()
		{
			InitializeComponent();
		}

		private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TimeControl control = obj as TimeControl;
			control.Value = new TimeSpan(control.Hours, control.Minutes, control.Seconds);
		}

		private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			TimeControl control = obj as TimeControl;
			control.Hours = ((TimeSpan)e.NewValue).Hours;
			control.Minutes = ((TimeSpan)e.NewValue).Minutes;
			control.Seconds = ((TimeSpan)e.NewValue).Seconds;
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			bool up = e.Delta > 0;

			switch (((Grid)sender).Name)
			{
				case "hour":
					Hours = Hours + (up ? 1 : -1);
					break;
				case "min":
					Minutes = Minutes + (up ? 1 : -1);
					break;
				case "sec":
					Seconds = Seconds + (up ? 1 : -1);
					break;
				default:
					throw new Exception("Not handled");
			}
		}
	}
}