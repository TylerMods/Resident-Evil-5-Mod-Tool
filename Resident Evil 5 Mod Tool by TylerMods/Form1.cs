using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiLib;

namespace Resident_Evil_5_Mod_Tool_by_TylerMods
{
    public partial class Form1 : Form
    {
        public Mem MemLib = new Mem();
        MultiConsoleAPI PC = new MultiConsoleAPI(SelectAPI.PCAPI);

        public Form1()
        {
            InitializeComponent();
            flatLabel2.Text = PC.ConnectTarget("re5") ? "Running" : "Not Running";
        }

        private void flatLabel1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void btnSetMoney_Click(object sender, EventArgs e)
        {
            PC.Extension.WriteInt32(0x1DB040D0, Convert.ToInt32(txtSetMoney.Text));
        }

        private void btnGodmodeSPChris_Click(object sender, EventArgs e)
        {
            //PC.Extension.WriteInt32(0x00DA283C, 0x24, 0x135c, 0, 1, 0);
        }

        private void btnMaxM9Ammo_Click(object sender, EventArgs e)
        {
            PC.Extension.WriteInt32(0x04AC15FC, 50);
        }

        private void btnSetPoints_Click(object sender, EventArgs e)
        {
            PC.Extension.WriteInt32(0x1DB040D4, Convert.ToInt32(txtSetPoints.Text));
        }
    }
}