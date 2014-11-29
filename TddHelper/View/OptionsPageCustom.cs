using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace DreamWorks.TddHelper.View
{
    /// <summary>
    /// Extends the standard dialog functionality for implementing ToolsOptions pages, 
    /// with support for the Visual Studio automation model, Windows Forms, and state 
    /// persistence through the Visual Studio settings mechanism.
    /// </summary>
	[Guid(GuidList.guidTddCustomOptionsPage)]
    public class OptionsPageCustom : DialogPage
    {
        #region Fields

        private TddHelperOptionsControl _optionsControl;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the window an instance of DialogPage that it uses as its user interface.
        /// </summary>
        /// <devdoc>
        /// The window this dialog page will use for its UI.
        /// This window handle must be constant, so if you are
        /// returning a Windows Forms control you must make sure
        /// it does not recreate its handle.  If the window object
        /// implements IComponent it will be sited by the 
        /// dialog page so it can get access to global services.
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                if (_optionsControl == null)
                {
					_optionsControl = new TddHelperOptionsControl();
                    _optionsControl.Location = new Point(0, 0);
                    _optionsControl.OptionsPage = this;
                }
                return _optionsControl;
            }
        }

        /// <summary>
        /// Gets or sets the path to the image file.
        /// </summary>
        /// <remarks>The property that needs to be persisted.</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CustomBitmap { get; set; }

        #endregion Properties

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_optionsControl != null)
                {
                    _optionsControl.Dispose();
                    _optionsControl = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Methods
    }
}
