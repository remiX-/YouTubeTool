﻿using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using YouTubeTool.Core;
using YouTubeTool.Utils.Messages;
using YouTubeTool.ViewModels;

namespace YouTubeTool.Windows
{
	public partial class WindowMain : Window
	{
		#region Vars
		private MainViewModel MyViewModel;
		#endregion

		public WindowMain()
		{
			InitializeComponent();

			MainSnackbar.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

			// Notification messages
			Messenger.Default.Register<ShowNotificationMessage>(this, m =>
			{
				if (m.CallbackCaption != null && m.Callback != null)
					MainSnackbar.MessageQueue.Enqueue(m.Message, m.CallbackCaption, m.Callback);
				else
					MainSnackbar.MessageQueue.Enqueue(m.Message);
			});
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as MainViewModel;

		}

		private void Window_Closed(object sender, EventArgs e)
		{

		}
	}
}
