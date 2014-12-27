using System;
using System.Windows.Forms;
using DreamWorks.TddHelper.ViewModel;
using MvvmFx.Windows.Data;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.View
{
	public partial class AssemblyAddReferenceOptionsControl : UserControl
	{
		private readonly BindingManager _bindingManager;
		private readonly OptionsViewModel _optionsViewModel;
		private static bool _bindingsAdded;

		public AddReferenceOptions OptionsPage { get; set; }

		public AssemblyAddReferenceOptionsControl()
		{
			InitializeComponent();
			_optionsViewModel = new OptionsViewModel();
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
			TddSettings.Default.Settings = JsonConvert.SerializeObject(_optionsViewModel);
			TddSettings.Default.Save();
			StaticOptions.TddHelper = _optionsViewModel;
		}

		public OptionsViewModel OptionsViewModel
		{
			get { return _optionsViewModel; }
		}

		private void UpdateUI()
		{
			// deserialization bypasses the property sets, thats why we have to update the UI
//			if (!string.IsNullOrEmpty(TddSettings.Default.Settings))
//			{
//				var fromDisk =
//					JsonConvert.DeserializeObject<OptionsViewModel>(TddSettings.Default.Settings);
//				_optionsViewModel.Clone(fromDisk);
//			}
//			_optionsViewModel.UpdateUI();
		}


		private void AddBindings()
		{
//			BindTextControls();
//			BindRadioButtons();
//			BindCheckboxes();
//			ClearcacheButton.Click += ClearcacheButton_Click;
//			clearProjectCacheButton.Click += ClearProjectcacheButton_Click;
		}
	}
}