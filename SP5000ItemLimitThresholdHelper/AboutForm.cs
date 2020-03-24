//using System.Linq;
using System.Windows.Forms;

namespace SP5000ItemLimitThresholdHelper
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            tbAbout.Text = @" *** Welcome to SP 5000 Item Limit Threshold Helper ***

Created by Ben Steinhauser of B&R Business Solutions.
   Updated in 2020 by Andrew Morgan of eTranservices.

";

            tbAbout.AppendText(" ");

        }
    }
}
