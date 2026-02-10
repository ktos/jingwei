using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

namespace Jingwei.PowerPointAddIn
{
    public partial class ManageTaskPaneRibbon
    {
        private void ManageTaskPaneRibbon_Load(object sender, RibbonUIEventArgs e) { }

        private void paneToggle_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.TaskPane.Visible = paneToggle.Checked;
        }
    }
}
