﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DreamWorks.TddHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DreamWorks.TddHelper.ViewModel
{
	public class AssociateTestProjectViewModel : ViewModelBase
	{
		private readonly RelayCommand _okCommand;
		private readonly RelayCommand _cancelCommand;
		private readonly ICanClose _view;
		private string _currentProject;
		private string _newProjectName;
		private ObservableCollection<string> _projects;
		private string _selectedProject;

		public AssociateTestProjectViewModel(ICanClose view, List<string> projectList, string currentProject)
		{
			_view = view;
			_okCommand = new RelayCommand(OnOk, IsOKEnabled);
			_cancelCommand = new RelayCommand(OnCancel);
			Projects = new ObservableCollection<string>(projectList);
			CurrentProject = currentProject;
		}

		private bool IsOKEnabled()
		{
			return true;
		}

		public ICommand CancelCommand
		{
			get { return _cancelCommand; }
		}

		public ICommand OkCommand
		{
			get { return _okCommand; }
		}

		public string CurrentProject
		{
			get { return _currentProject; }
			set
			{
				_currentProject = value;
				RaisePropertyChanged(() => CurrentProject);
			}
		}

		public string NewProjectName
		{
			get { return _newProjectName; }
			set
			{
				_newProjectName = value;
				if (!string.IsNullOrEmpty(SelectedProject))
					SelectedProject = string.Empty;
				RaisePropertyChanged(() => NewProjectName);
			}
		}

		public ObservableCollection<string> Projects
		{
			get { return _projects; }
			set
			{
				_projects = value;
				RaisePropertyChanged(() => Projects);
			}
		}

		public string SelectedProject
		{
			get { return _selectedProject; }
			set
			{
				_selectedProject = value;
				NewProjectName = string.Empty;
				RaisePropertyChanged(() => SelectedProject);
			}
		}

		private void OnCancel()
		{
			_view.CloseWindow(true);
		}

		private void OnOk()
		{
			_view.CloseWindow();
		}

		public void OnLoaded()
		{
			RaisePropertyChanged(() => Projects);
			RaisePropertyChanged(() => CurrentProject);
		}
	}
}