
using System.Collections.Generic;
using System.Windows;
using DreamWorks.TddHelper.ViewModel;

namespace DreamWorks.TddHelper.View
{
	/// <summary>
	/// Interaction logic for ResolveFileConflictDialog.xaml
	/// </summary>
	public partial class ResolveFileConflictDialog : Window, ICanClose
	{
		private readonly ResolveFileConflictDialogViewModel _viewModel;

		public ResolveFileConflictDialog(IEnumerable<string> fileList)
		{
			InitializeComponent();
			_viewModel = new ResolveFileConflictDialogViewModel(this, fileList);
			DataContext = ViewModel;
		}

		public ResolveFileConflictDialogViewModel ViewModel
		{
			get { return _viewModel; }
		}

		public void CloseWindow(bool isCancel = false)
		{
			DialogResult = !isCancel;
			Close();
		}

		private void ResolveFileConflictDialog_OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewModel.OnLoaded();
		}
	}
}