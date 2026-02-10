using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jingwei.PowerPointAddIn
{
    public partial class JingweiStatus : UserControl
    {
        public JingweiStatus()
        {
            InitializeComponent();
        }

        internal void Log(string message)
        {
            status.Invoke(new MethodInvoker(() => status.Items.Add(message)));
        }
    }
}
