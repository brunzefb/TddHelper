using System;
using System.IO;
using System.Windows.Forms;
using DreamWorks.TddHelper.ViewModel;
using MvvmFx.Windows.Data;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.View
{
	public partial class AssemblyAddReferenceOptionsControl : UserControl
	{
		private readonly BindingManager _bindingManager;
		private readonly AddReferencesOptionsViewModel _viewModel;
		private static bool _bindingsAdded;

		public AddReferenceOptions AddReferenceOptionsPage { get; set; }

		public AssemblyAddReferenceOptionsControl()
		{
			InitializeComponent();
			_viewModel = new AddReferencesOptionsViewModel();
			_bindingManager = new BindingManager();
		}

		public void OnLoad(object sender, EventArgs e)
		{
			if (!_bindingsAdded)
			{
				AddBindings();
				_bindingsAdded = true;
			}
			UpdateUI();
		}
		public void Save()
		{
			TddSettings.Default.AddAssemblySettings = JsonConvert.SerializeObject(_viewModel);
			TddSettings.Default.Save();
			StaticOptions.ReferencesOptions = _viewModel;
		}

		public AddReferencesOptionsViewModel ViewModel
		{
			get { return _viewModel; }
		}

		private void UpdateUI()
		{
			// deserialization bypasses the property sets, thats why we have to update the UI
			if (!string.IsNullOrEmpty(TddSettings.Default.AddAssemblySettings))
			{
				var fromDisk =
					JsonConvert.DeserializeObject<AddReferencesOptionsViewModel>(TddSettings.Default.AddAssemblySettings);
				_viewModel.Clone(fromDisk);
			}
			_viewModel.UpdateUI();
		}

		private void AddBindings()
		{
			BindTextControls();
			BindRadioButtons();
			BrowseButton.Click += BrowseButton_Click;
		}

		void BrowseButton_Click(object sender, EventArgs e)
		{
			openFileDialog.InitialDirectory = "c:\\";
			openFileDialog.Filter = "Assembly files (*.dll)|*.dll";
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			DialogResult dr = openFileDialog.ShowDialog(this);
			if (dr != DialogResult.OK || !openFileDialog.CheckFileExists)
				return;
			_viewModel.AssemblyPath = openFileDialog.FileName;
		}

		private void BindRadioButtons()
		{
			_bindingManager.Bindings.Add(
				new TypedBinding<RadioButton, AddReferencesOptionsViewModel>
					(UseNuGetRadio, r => r.Checked, _viewModel,
						o => o.UseNuGet));
			_bindingManager.Bindings.Add(
				new TypedBinding<RadioButton, AddReferencesOptionsViewModel>
					(UseFileRadio, r => r.Checked, _viewModel,
						o => o.UseFileAssembly));
		}

		private void BindTextControls()
		{
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, AddReferencesOptionsViewModel>
					(PackageIdTextBox, t => t.Text, _viewModel,
						o => o.PackageId));
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, AddReferencesOptionsViewModel>
					(VersionMajorTextBox, t => t.Text, _viewModel,
						o => o.VersionMajor));
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, AddReferencesOptionsViewModel>
					(VersionMinorTextBox, t => t.Text, _viewModel,
						o => o.VersionMinor));
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, AddReferencesOptionsViewModel>
					(VersionBuildTextBox, t => t.Text, _viewModel,
						o => o.VersionBuild));
			_bindingManager.Bindings.Add(
				new TypedBinding<Label, AddReferencesOptionsViewModel>
					(LabelAssembly, t => t.Text, _viewModel,
						o => o.ViewAssemblyPath));
		}
	}
}