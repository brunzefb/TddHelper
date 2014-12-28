
using System.Collections.Generic;
using System.Windows;

using DreamWorks.TddHelper.ViewModel;

namespace DreamWorks.TddHelper.View
{
	/// <summary>
	/// Interaction logic for AssociateTestProject.xaml
	/// </summary>
	public partial class AssociateTestProject : Window, ICanClose
	{
		private readonly AssociateTestProjectViewModel _viewModel;
		
		public AssociateTestProject(string currentProject)
		{
			InitializeComponent();
			_viewModel = new AssociateTestProjectViewModel(this, currentProject);
			DataContext = ViewModel;
		}

		public AssociateTestProjectViewModel ViewModel
		{
			get { return _viewModel; }
		}

		public void CloseWindow(bool isCancel = false)
		{
			DialogResult = !isCancel;
			Close();
		}

		public string SelectedProject
		{
			get { return _viewModel.SelectedProject.Path; }
		}

		public string NewProjectName
		{
			get { return _viewModel.NewProjectName; }
		}

		private void AssociateTestProject_OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewModel.OnLoaded();
		}
	}
}