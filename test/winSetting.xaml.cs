using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Panuon.WPF.UI;

namespace VPET.Evian.AutoBuy
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        SettingPP vts;


        public winSetting(SettingPP vts)
        {
            InitializeComponent();
            this.vts = vts;
            if (vts.MW.Set.AutoBuy && vts.Set.Enable) 
            {
                vts.MW.Set.AutoBuy = false;
                MessageBoxX.Show("桌宠自带的自动购买和本mod的自动购买冲突，已关闭桌宠自带的自动购买".Translate(), "错误".Translate(), Panuon.WPF.UI.MessageBoxIcon.Error);
            }
            SwitchOn.IsChecked = vts.Set.Enable;
            Mode.IsChecked = vts.Set.Mode;
            MaxPrice.Text = vts.Set.MaxPrice.ToString();
            LowDeposit.Text = vts.Set.MinDeposit.ToString();
            MinThirst.Text = vts.Set.MinThirst.ToString();
            MinSatiety.Text = vts.Set.MinSatiety.ToString();
            MinMood.Text = vts.Set.MinMood.ToString();
            MinHealth.Text = vts.Set.MinHealth.ToString();
            MinGoodThirst.Text = (vts.Set.MinGoodThirst - 2).ToString();
            MinGoodSatiety.Text = (vts.Set.MinGoodSatiety - 2).ToString();
            MinGoodMood.Text = (vts.Set.MinGoodMood - 2).ToString();
            MinGoodHealth.Text = (vts.Set.MinGoodHealth - 2).ToString();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (vts.Set.Enable != SwitchOn.IsChecked.Value)
            {
                if (SwitchOn.IsChecked.Value && vts.MW.Set.AutoBuy) 
                {
                    vts.MW.Set.AutoBuy = false;
                    MessageBoxX.Show("桌宠自带的自动购买和本mod的自动购买冲突，已关闭桌宠自带的自动购买".Translate(), "错误".Translate(), Panuon.WPF.UI.MessageBoxIcon.Error);
                }
                vts.Set.Enable = SwitchOn.IsChecked.Value;
            }
            if (vts.Set.Mode != Mode.IsChecked.Value) 
            {
                vts.Set.Mode = Mode.IsChecked.Value;
            }
            vts.Set.MaxPrice = Convert.ToInt32(MaxPrice.Text);
            vts.Set.MinDeposit = Convert.ToInt32(LowDeposit.Text);
            vts.Set.MinThirst = Convert.ToInt32(MinThirst.Text);
            vts.Set.MinSatiety = Convert.ToInt32(MinSatiety.Text);
            vts.Set.MinMood = Convert.ToInt32(MinMood.Text);
            vts.Set.MinHealth = Convert.ToInt32(MinHealth.Text);
            vts.Set.MinGoodThirst = Convert.ToInt32(MinGoodThirst.Text)+2;
            vts.Set.MinGoodSatiety = Convert.ToInt32(MinGoodSatiety.Text)+2;
            vts.Set.MinGoodMood = Convert.ToInt32(MinGoodMood.Text)+2;
            vts.Set.MinGoodHealth = Convert.ToInt32(MinGoodHealth.Text)+2;
            vts.MW.Set["SettingPP"] = LPSConvert.SerializeObject(vts.Set, "SettingPP");
            Close();
        }

    }
}
