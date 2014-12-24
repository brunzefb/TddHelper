using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using DreamWorks.TddHelper.Model;
using DreamWorks.TddHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DreamWorks.TddHelper.ViewModel
{
	public class AssociateTestProjectViewModel : ViewModelBase
	{
		private readonly RelayCommand _okCommand;
		private readonly RelayCommand _cancelCommand;
		private readonly RelayCommand _createNewProjectCommand;
		private readonly ICanClose _view;
		private readonly string _currentProject;
		private string _newProjectName;
		private DisplayPathHelper _selectedProject;
		private readonly CachedProjectAssociations _cachedProjectAssociations;
		private readonly bool _isSourcePathTest;
		private readonly ObservableCollection<DisplayPathHelper> _projectList;
		public bool RequestCreateProject { get; set; }

		public AssociateTestProjectViewModel(ICanClose view, IEnumerable<string> projectList, 
			string currentProject, CachedProjectAssociations cachedProjectAssociations, bool isSourcePathTest)
		{
			_view = view;
			_okCommand = new RelayCommand(OnOk, IsOKEnabled);
			_cancelCommand = new RelayCommand(OnCancel);
			_createNewProjectCommand = new RelayCommand(OnCreateNewProject, IsNewProjectCreationAllowed);

			var list = new List<DisplayPathHelper>();
			foreach (var project in projectList)
			{
				var display = new DisplayPathHelper
				{
					Path = project,
					DisplayPath = Path.GetFileNameWithoutExtension(project)
				};
				list.Add(display);
			}
			_projectList = new ObservableCollection<DisplayPathHelper>(list);	
			_currentProject = currentProject;
			_cachedProjectAssociations = cachedProjectAssociations;
			_isSourcePathTest = isSourcePathTest;
		}

		private bool IsNewProjectCreationAllowed()
		{
			return _projectList.Any(x=>
			{
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x.Path);
				return fileNameWithoutExtension != null && fileNameWithoutExtension.Contains(_newProjectName);
			});
		}

		private void OnCreateNewProject()
		{
			RequestCreateProject = true;
			_view.CloseWindow();
		}

		private bool IsOKEnabled()
		{
			return _selectedProject != null;
		}

		public ICommand CancelCommand
		{
			get { return _cancelCommand; }
		}

		public ICommand CreateNewProjectCommand
		{
			get { return _createNewProjectCommand; }
		}

		public ICommand OkCommand
		{
			get { return _okCommand; }
		}

		public string CurrentProject
		{
			get { return Path.GetFileNameWithoutExtension(_currentProject); }
			
		}

		public string NewProjectName
		{
			get { return _newProjectName; }
			set
			{
				_newProjectName = value;
				_createNewProjectCommand.RaiseCanExecuteChanged();
				RaisePropertyChanged(() => NewProjectName);
			}
		}

		public ObservableCollection<DisplayPathHelper> Projects
		{
			get { return _projectList; }
		}

		public DisplayPathHelper SelectedProject
		{
			get { return _selectedProject; }
			set
			{
				if (_selectedProject == value)
					return;
				_selectedProject = value;
				
				UpdateOkButtonUsingOneShotTimer();
			}
		}

		private void UpdateOkButtonUsingOneShotTimer()
		{
			var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
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

		private void OnCancel()
		{
			_view.CloseWindow(true);
		}

		private void OnOk()
		{
			if (_isSourcePathTest)
				_cachedProjectAssociations.AddAssociation(SelectedProject.Path, _currentProject);
			else
				_cachedProjectAssociations.AddAssociation(_currentProject, SelectedProject.Path);
			_cachedProjectAssociations.Save();
			_view.CloseWindow();
		}

		public void OnLoaded()
		{
			RaisePropertyChanged(() => Projects);
			RaisePropertyChanged(() => CurrentProject);
		}
	}
}