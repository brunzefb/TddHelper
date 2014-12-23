using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using DreamWorks.TddHelper.Utility;
using DreamWorks.TddHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;


namespace DreamWorks.TddHelper.ViewModel
{
	public class DisplayPathHelper
	{
		public string Path { get; set; }
		public string DisplayPath { get; set; }
	}

	public class ResolveFileConflictDialogViewModel : ViewModelBase
	{
		private const int MaxPathCharacters = 90;
		private readonly RelayCommand _okCommand;
		private readonly RelayCommand _cancelCommand;
		private readonly ICanClose _view;
		private DisplayPathHelper _selectedFile;
		private readonly ObservableCollection<DisplayPathHelper> _fileList;

		public ResolveFileConflictDialogViewModel(ICanClose view, IEnumerable<string> fileList)
		{
			_okCommand = new RelayCommand(OnOk, IsOKEnabled);
			_cancelCommand = new RelayCommand(OnCancel);
			_view = view;
			var list = new List<DisplayPathHelper>();
			foreach (var file in fileList)
			{
				var display = new DisplayPathHelper();
				display.Path = file;
				display.DisplayPath = PathUtil.ShortenPath(RelativePathHelper.GetRelativePath(file), MaxPathCharacters);
				list.Add(display);
			}
			_fileList = new ObservableCollection<DisplayPathHelper>(list);
		}

		private bool IsOKEnabled()
		{
			return _selectedFile != null;
		}

		public ICommand CancelCommand
		{
			get { return _cancelCommand; }
		}

		public ICommand OkCommand
		{
			get { return _okCommand; }
		}

		private void OnCancel()
		{
			_view.CloseWindow(true);
		}

		private void OnOk()
		{
			_view.CloseWindow();
		}
		
		public DisplayPathHelper SelectedFile
		{
			get { return _selectedFile; }
			set
			{
				_selectedFile = value;
				RaisePropertyChanged(() => SelectedFile);
				UpdateOkButtonUsingOneShotTimer();
			}
		}

		private void UpdateOkButtonUsingOneShotTimer()
		{
			var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(20)};
			timer.Tick += timer_Tick;
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			var timer = sender as DispatcherTimer;
			if (timer != null)
				timer.Stop();
			_okCommand.RaiseCanExecuteChanged();

		}
		public ObservableCollection<DisplayPathHelper> FileList
		{
			get { return _fileList; }
		}

		public void OnLoaded()
		{
			_okCommand.RaiseCanExecuteChanged();
		}
	}
}