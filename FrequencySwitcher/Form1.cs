using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrequencySwitcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            switch (Program.errorStatus)
            {
                case 0:
                    label1.Text = "No arguments were passed to FrequencySwitcher.";
                    break;
                case 1:
                    label1.Text = "The values passed do not correspond to a valid configuration on your monitor.";
                    break;
                default:
                    label1.Text = "Unknown error code was passed. Contact GMR.";
                    break;
            }
        }
    }
}
