using System;
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
	[Guid(GuidList.guidAddReferencesOptionsPage)]
    public class AddReferenceOptions : DialogPage
    {
        private AssemblyAddReferenceOptionsControl _addReferencesOptionsControl;

	    protected override void OnActivate(CancelEventArgs e)
	    {
		    base.OnActivate(e);
			_addReferencesOptionsControl.OnLoad(this, e);
	    }

	    protected override void OnClosed(EventArgs e)
	    {
		    base.OnClosed(e);
		    _addReferencesOptionsControl.Save();
		   
	    }

	    /// <summary>
        /// Gets the window an instance of DialogPage that it uses as its user interface.
        /// </summary>
        /// <devdoc>
		/// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                if (_addReferencesOptionsControl == null)
                {
					_addReferencesOptionsControl = new AssemblyAddReferenceOptionsControl();
                    _addReferencesOptionsControl.Location = new Point(0, 0);
                    _addReferencesOptionsControl.AddReferenceOptionsPage = this;
                }
                return _addReferencesOptionsControl;
            }
        }

        /// <summary>
        /// Gets or sets the path to the image file.
        /// </summary>
        /// <remarks>The property that needs to be persisted.</remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CustomBitmap { get; set; }
		
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_addReferencesOptionsControl != null)
                {
                    _addReferencesOptionsControl.Dispose();
                    _addReferencesOptionsControl = null;
                }
            }
            base.Dispose(disposing);
        }
	}
}
