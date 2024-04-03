using Accessibility;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.GraphInfo;


namespace VPET.Evian.TEST
{
    public class SettingPP : MainPlugin
    {
        public Setting Set;

        public override string PluginName => "SettingPP";
        public SettingPP(IMainWindow mainwin) : base(mainwin)
        {
        }
        public override void LoadPlugin()
        {
            Set = new Setting(MW.Set["SettingPP"]);
            Set.MaxPrice = MW.Set["SettingPP"].GetInt("MaxPrice");
            Set.MinThirst = MW.Set["SettingPP"].GetInt("MinThirst");
            Set.MinSatiety = MW.Set["SettingPP"].GetInt("MinSatiety");
            Set.MinMood = MW.Set["SettingPP"].GetInt("MinMood");
            Set.MinHealth = MW.Set["SettingPP"].GetInt("MinHealth");
            Set.MinDeposit = MW.Set["SettingPP"].GetInt("MinDeposit");
            Set.MinGoodThirst = MW.Set["SettingPP"].GetInt("MinGoodThirst");
            Set.MinGoodSatiety = MW.Set["SettingPP"].GetInt("MinGoodSatiety");
            Set.MinGoodMood = MW.Set["SettingPP"].GetInt("MinGoodMood");
            Set.MinGoodHealth = MW.Set["SettingPP"].GetInt("MinGoodHealth");
            Set.Enable = MW.Set["SettingPP"].GetBool("Enable");

            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "SettingPP".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
            MW.Main.FunctionSpendHandle += AutoBuyPP;
            ///base.LoadPlugin();
        }
        public winSetting winSetting;
        public override void Setting()
        {
            if (winSetting == null)
            {
                winSetting = new winSetting(this);
                winSetting.Show();
            }
            else
            {
                winSetting.Topmost = true;
            }
        }
        public List<Food> Foods { get; } = new List<Food>();
        private void TakeItem(Food item)
        {
            //通知
            item.LoadEatTimeSource(MW);
            item.NotifyOfPropertyChange("Description");

            MW.GameSavesData.GameSave.Money -= item.Price;
            //统计
            MW.GameSavesData.Statistics[(gint)"stat_buytimes"]++;
            MW.GameSavesData.Statistics[(gint)("buy_" + item.Name)]++;
            MW.GameSavesData.Statistics[(gdbe)"stat_betterbuy"] += item.Price;
            MW.GameSavesData.GameSave.EatFood(item);
            switch (item.Type)
            {
                case Food.FoodType.Food:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_food"] += item.Price;
                    break;
                case Food.FoodType.Drink:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_drink"] += item.Price;
                    break;
                case Food.FoodType.Drug:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_drug"] += item.Price;
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_drug_exp"] += item.Exp;
                    break;
                case Food.FoodType.Snack:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_snack"] += item.Price;
                    break;
                case Food.FoodType.Functional:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_functional"] += item.Price;
                    break;
                case Food.FoodType.Meal:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_meal"] += item.Price;
                    break;
                case Food.FoodType.Gift:
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_gift"] += item.Price;
                    MW.GameSavesData.Statistics[(gdbe)"stat_bb_gift_like"] += item.Likability;
                    break;
            }
            MW.GameSavesData.Statistics[(gint)"stat_autobuy"]++;
            MW.Main.Display(item.GetGraph(), item.ImageSource, MW.Main.DisplayToNomal);
        }
        private void AutoBuyPP()
        {
            var sm = MW.GameSavesData.GameSave.StrengthMax;
            var smood = MW.GameSavesData.GameSave.FeelingMax;
            if (Set.Enable && MW.GameSavesData.GameSave.Money >= (Set.MinDeposit > 100 ? Set.MinDeposit : 100))
            {
                var havemoney = (Set.MaxPrice < MW.GameSavesData.GameSave.Money ? Set.MaxPrice : MW.GameSavesData.GameSave.Money);
                List<Food> food = MW.Foods.FindAll(x => x.Price >= 2 && x.Health >= 0 && x.Exp >= 0 && x.Likability >= 0 && x.Price < havemoney && x.Feeling > 0 //桌宠不吃负面的食物
                 && !x.IsOverLoad() // 不吃超模食物
                ) ;

                if (MW.GameSavesData.GameSave.StrengthFood < sm * Set.MinSatiety * 0.01)
                {
                    if (MW.GameSavesData.GameSave.StrengthFood < sm * Set.MinSatiety * 0.01 * 0.8)
                    {//太饿了,找正餐
                        food = food.FindAll(x => x.Type == Food.FoodType.Meal && x.StrengthFood > (sm * Set.MinGoodSatiety * 0.01 < sm ? (sm * Set.MinGoodSatiety * 0.01) : sm));
                    }
                    else
                    {//找零食
                        food = food.FindAll(x => x.Type == Food.FoodType.Snack && x.StrengthFood > (sm * Set.MinGoodSatiety * 0.01 < sm ? (sm * Set.MinGoodSatiety * 0.01) : sm));
                    }
                    if (food.Count == 0)
                        return;
                    var item = food[Function.Rnd.Next(food.Count)];
                    MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                    TakeItem(item);
                }
                else if (MW.GameSavesData.GameSave.StrengthDrink < sm * Set.MinThirst * 0.01)
                {
                    food = food.FindAll(x => x.Type == Food.FoodType.Drink && x.StrengthDrink > (sm * Set.MinGoodThirst * 0.01 < sm ? sm * Set.MinGoodThirst * 0.01 : sm));
                    if (food.Count == 0)
                        return;
                    var item = food[Function.Rnd.Next(food.Count)];
                    MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                    TakeItem(item);
                }
                else if (MW.GameSavesData.GameSave.Health < 100 * Set.MinHealth * 0.01)
                {
                health:
                    food = food.FindAll(x => x.Type == Food.FoodType.Drug && x.Health > (sm * Set.MinGoodHealth * 0.01 < sm ? sm * Set.MinGoodHealth * 0.01 : sm));
                    if (food.Count == 0)
                    {
                        sm *= 0.5;
                        goto health;
                    }
                    var item = food[Function.Rnd.Next(food.Count)];
                    MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                    TakeItem(item);
                }
                else if (MW.GameSavesData.GameSave.Feeling < smood * Set.MinMood * 0.01) 
                {
                    if (MW.GameSavesData.GameSave.Feeling < smood * Set.MinMood * 0.01 * 0.8) 
                    {
                    mood_Gift:
                        food = food.FindAll(x => x.Type == Food.FoodType.Gift && x.Feeling > (smood * Set.MinGoodMood * 0.01 < smood ? smood * Set.MinGoodMood * 0.01 : smood));
                        if (food.Count == 0)
                        {
                            smood *= 0.5;
                            goto mood_Gift;
                        }
                        var item = food[Function.Rnd.Next(food.Count)];
                        MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                        TakeItem(item);
                    }
                    else
                    {
                    mood_Snake:
                        food = food.FindAll(x => x.Type == Food.FoodType.Snack && x.Feeling > (smood * Set.MinGoodMood * 0.01 < smood ? smood * Set.MinGoodMood * 0.01 : smood));
                        if (food.Count == 0)
                        {
                            smood *= 0.8;
                            goto mood_Snake;
                        }
                        var item = food[Function.Rnd.Next(food.Count)];
                        MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                        TakeItem(item);
                    }
                }
                else 
                {
                    return;
                }
            }
            return;
        }
    }
}

