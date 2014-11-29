using System;
using System.Windows.Forms;
using DreamWorks.TddHelper.ViewModel;
using MvvmFx.Windows.Data;
using Newtonsoft.Json;
using DreamWorks.TddHelper;

namespace DreamWorks.TddHelper.View
{
	public partial class TddHelperOptionsControl : UserControl
	{
		private BindingManager _bindingManager;
		private OptionsViewModel _optionsViewModel;

		public TddHelperOptionsControl()
		{
			InitializeComponent();
		}

		public void OnLoad(object sender, EventArgs e)
		{
			AddBindings();
			LoadOrDefault();
		}

		public void Save()
		{
			TddSettings.Default.Settings = JsonConvert.SerializeObject(_optionsViewModel);
			TddSettings.Default.Save();
		}

		private void LoadOrDefault()
		{
			if (string.IsNullOrEmpty(TddSettings.Default.Settings))
			{
				SetDefaults();
				Save();
			}
			else
				_optionsViewModel = JsonConvert.DeserializeObject<OptionsViewModel>
					(TddSettings.Default.Settings);
		}

		private void SetDefaults()
		{
			_optionsViewModel.ProjectSuffix = "Test.csproj";
			_optionsViewModel.TestFileSuffix = "Test.cs";
			_optionsViewModel.UnitTestLeft = true;
			_optionsViewModel.UnitTestRight = false;
			_optionsViewModel.NoSplit = false;
			_optionsViewModel.AutoCreateTestFile = false;
			_optionsViewModel.AutoCreateTestProject = false;
			_optionsViewModel.CreateReference = true;
			_optionsViewModel.MakeFriendAssembly = true;
			_optionsViewModel.MirrorProjectFolders = true;
			_optionsViewModel.Clean = false;
		}

		private void AddBindings()
		{
			_optionsViewModel = new OptionsViewModel();
			_bindingManager = new BindingManager();

			BindTextControls();
			BindRadioButtons();
			BindCheckboxes();

			ClearcacheButton.Click += ClearcacheButton_Click;
		}

		void ClearcacheButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show(OptionsPage, 
				Resources.TddHelperOptionsControl_ConfirmClearCacheMessage, 
				Resources.TddHelper_App_Name, MessageBoxButtons.OK);
		}

		private void BindCheckboxes()
		{
			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(AutoCreateProjectCheckbox, c => c.Checked, _optionsViewModel,
						o => o.AutoCreateTestProject));
			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(AutoCreateFileCheckbox, c => c.Checked, _optionsViewModel,
						o => o.AutoCreateTestFile));
			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(MirrorProjectFoldersChecbox, c => c.Checked, _optionsViewModel,
						o => o.MirrorProjectFolders));
			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(CreateReferenceCheckbox, c => c.Checked, _optionsViewModel,
						o => o.CreateReference));
			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(MakeFriendAssemblyCheckbox, c => c.Checked, _optionsViewModel,
						o => o.MakeFriendAssembly));

			_bindingManager.Bindings.Add(
				new TypedBinding<CheckBox, OptionsViewModel>
					(CleanCheckbox, c => c.Checked, _optionsViewModel,
						o => o.Clean));
		}

		private void BindRadioButtons()
		{
			_bindingManager.Bindings.Add(
				new TypedBinding<RadioButton, OptionsViewModel>
					(UnitTestLeftRadio, r => r.Checked, _optionsViewModel,
						o => o.UnitTestLeft));
			_bindingManager.Bindings.Add(
				new TypedBinding<RadioButton, OptionsViewModel>
					(UnitTestRightRadio, r => r.Checked, _optionsViewModel,
						o => o.UnitTestRight));
			_bindingManager.Bindings.Add(
				new TypedBinding<RadioButton, OptionsViewModel>
					(NoSplitRadio, r => r.Checked, _optionsViewModel,
						o => o.NoSplit));
		}

		private void BindTextControls()
		{
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, OptionsViewModel>
					(TestFileSuffixEdit, t => t.Text, _optionsViewModel,
						o => o.TestFileSuffix));
			_bindingManager.Bindings.Add(
				new TypedBinding<TextBox, OptionsViewModel>
					(ProjectSuffixEdit, t => t.Text, _optionsViewModel,
						o => o.ProjectSuffix));
		}

		public OptionsPageCustom OptionsPage { get; set; }

		
	}
}