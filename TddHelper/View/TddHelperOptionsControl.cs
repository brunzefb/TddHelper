using System;
using System.Windows.Forms;
using DreamWorks.TddHelper.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using MvvmFx.Windows.Data;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.View
{
	public class OptionsClearCache
	{
	}

	public partial class TddHelperOptionsControl : UserControl
	{
		private readonly BindingManager _bindingManager;
		private OptionsViewModel _optionsViewModel;
		private static bool _bindingsAdded;

		public TddHelperOptionsControl()
		{
			InitializeComponent();
			_optionsViewModel = new OptionsViewModel();
			_bindingManager = new BindingManager();
		}

		public OptionsPageCustom OptionsPage { get; set; }

		public void OnLoad(object sender, EventArgs e)
		{
			if (!_bindingsAdded)
			{
				AddBindings();
				_bindingsAdded = true;
			}
			UpdateUI();
		}

		public OptionsViewModel OptionsViewModel
		{
			get { return _optionsViewModel; }
		}

		public void Save()
		{
			TddSettings.Default.Settings = JsonConvert.SerializeObject(_optionsViewModel);
			TddSettings.Default.Save();
			StaticOptions.TddHelper = _optionsViewModel;
		}

		private void UpdateUI()
		{
			// deserialization bypasses the property sets, thats why we have to update the UI
			if (!string.IsNullOrEmpty(TddSettings.Default.Settings))
			{
				var fromDisk = JsonConvert.DeserializeObject<OptionsViewModel>(TddSettings.Default.Settings);
				_optionsViewModel.Clone(fromDisk);
			}
			_optionsViewModel.UpdateUI();
		}

		private void AddBindings()
		{
			BindTextControls();
			BindRadioButtons();
			BindCheckboxes();
			ClearcacheButton.Click += ClearcacheButton_Click;
		}

		void ClearcacheButton_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(OptionsPage, 
				Resources.TddHelperOptionsControl_ConfirmClearCacheMessage, 
				Resources.TddHelper_App_Name, MessageBoxButtons.YesNo);
			if (result == DialogResult.Yes)
				Messenger.Default.Send(new OptionsClearCache());

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
	}
}