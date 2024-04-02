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
    public class GOOD
    {
        public Food Food { get; set; }
        private double Weight = 0;
        public GOOD(Food food,int weight)
        {
            Food = food;
            Weight = weight;
        }
        public GOOD() { }
        public double GetWeight()
        {
            return Weight;
        }
        public bool SetWeight(Food food)
        {
            if (food.Type == Food.FoodType.Meal || food.Type == Food.FoodType.Food)
            {
                Weight = food.StrengthFood / food.Price;
            }
            else if (food.Type == Food.FoodType.Drink)
            {
                Weight = food.StrengthDrink / food.Price;
            }
            else if (food.Type == Food.FoodType.Gift)
            {
                Weight = food.Feeling / food.Price;
            }
            else if (food.Type == Food.FoodType.Snack)
            {
                Weight = (food.StrengthFood * 0.4 + food.Feeling * 0.6) / food.Price;
            }
            else if (food.Type == Food.FoodType.Drug)
            {
                Weight = food.Health / food.Price;
            }
            else 
                return false;
            return true;
        }
    }
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
            Set.MinDeposit = MW.Set["SettingPP"].GetInt("MinDeposit");
            Set.MinGoodThirst = MW.Set["SettingPP"].GetInt("MinGoodThirst");
            Set.MinGoodSatiety = MW.Set["SettingPP"].GetInt("MinGoodSatiety");
            Set.MinGoodMood = MW.Set["SettingPP"].GetInt("MinGoodMood");
            Set.Enable = MW.Set["SettingPP"].GetBool("Enable");

            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "SettingPP",
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
            GOOD tmp=new GOOD();
            foreach (var item in MW.Foods)
            {
                if (tmp.SetWeight(item))
                { 
                    tmp.Food = item;
                    Goods.Add(tmp);
                }
            }
            if (Goods.Count == 0) MessageBox.Show("ERROR,No Good Has Been get");
            tmp = null;
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
        public List<GOOD> Goods { get; } = new List<GOOD>();
        private void TakeItem(Food item)
        {
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
        public void RAND(List<GOOD> good, GOOD result)
        {
            Random rand = new Random();
            var items = good.ToList();
            var totalWeight = items.Sum(x => x.GetWeight());
            var randomWeightedIndex = Convert.ToDouble(rand.Next(Convert.ToInt32(totalWeight * 1000)))/1000;
            var itemWeightedIndex = 0.00;
            MessageBox.Show("GOT");
            foreach (var item in items)
            {
                itemWeightedIndex += item.GetWeight();
                if (randomWeightedIndex < itemWeightedIndex)
                {
                    result = item;
                    rand = null;
                    if(result.Food.Price!=0) MessageBox.Show("GOT!");
                    return;
                }
            }
            rand = null;
            MessageBox.Show("ERROR,Falied To Get Random");
        }
        private void AutoBuyPP()
        {
            var sm = MW.GameSavesData.GameSave.StrengthMax;
            var smood = MW.GameSavesData.GameSave.FeelingMax;
            if (Set.Enable && MW.GameSavesData.GameSave.Money >= (Set.MinDeposit > 100 ? Set.MinDeposit : 100))
            {
                var havemoney = (Set.MaxPrice < MW.GameSavesData.GameSave.Money ? Set.MaxPrice : MW.GameSavesData.GameSave.Money);
                List<GOOD> good = Goods.FindAll(x =>  x.Food.Price < havemoney //桌宠不吃负面的食物
                );
                GOOD result = new GOOD();
                result.Food.Price = 0;
                if (good.Count != 0) MessageBox.Show("Good Is Selected");
                if (MW.GameSavesData.GameSave.StrengthFood < sm * Set.MinSatiety * 0.01)
                {
                    if (MW.GameSavesData.GameSave.StrengthFood < sm * Set.MinSatiety * 0.01 * 0.8)
                    {
                        good = good.FindAll(x => x.Food.Type == Food.FoodType.Meal && x.Food.StrengthFood > (sm * Set.MinGoodSatiety * 0.01 < sm ? sm * Set.MinGoodSatiety * 0.01 : sm));
                        if (good.Count == 0)
                        {
                            result = null;
                            return;
                        }
                        RAND(good, result);
                    }
                    else
                    {
                        good = good.FindAll(x => x.Food.Type == Food.FoodType.Snack && x.Food.StrengthFood > (sm * Set.MinGoodSatiety * 0.01 < sm ? sm * Set.MinGoodSatiety * 0.01 : sm));
                        if (good.Count == 0)
                        {
                            result = null;
                            return;
                        }
                        RAND(good, result);
                    }
                    if (result.Food.Price == 0) MessageBox.Show("ERROR,No Food Is Randomed");
                    MW.GameSavesData.GameSave.Money -= result.Food.Price * 0.2;
                    TakeItem(result.Food);
                }
                else if (MW.GameSavesData.GameSave.StrengthDrink < sm * Set.MinThirst * 0.01)
                {
                    good = good.FindAll(x => x.Food.Type == Food.FoodType.Drink && x.Food.StrengthDrink > (sm * Set.MinGoodThirst * 0.01 < sm ? sm * Set.MinGoodThirst * 0.01 : sm));
                    if (good.Count == 0)
                    {
                        result = null;
                        return;
                    }
                    RAND(good, result);
                    if (result.Food.Price == 0) MessageBox.Show("ERROR,No Food Is Randomed");
                    MW.GameSavesData.GameSave.Money -= result.Food.Price * 0.2;
                    TakeItem(result.Food);
                }
                else if (MW.GameSavesData.GameSave.Feeling < smood * Set.MinMood * 0.01) 
                {
                    if (MW.GameSavesData.GameSave.Feeling < smood * Set.MinMood * 0.01 * 0.8)
                    {
                    mood_Gift:
                        good = good.FindAll(x => x.Food.Type == Food.FoodType.Gift && x.Food.Feeling > (sm * Set.MinGoodMood * 0.01 < sm ? sm * Set.MinGoodMood * 0.01 : sm));
                        if (good.Count == 0)
                        {
                            smood *= 0.5;
                            goto mood_Gift;
                        }
                        RAND(good, result);
                    }
                    else
                    {
                    mood_Snake:
                        good = good.FindAll(x => x.Food.Type == Food.FoodType.Snack && x.Food.Feeling > (sm * Set.MinGoodMood * 0.01 < sm ? sm * Set.MinGoodMood * 0.01 : sm));
                        if (good.Count == 0)
                        {
                            smood *= 0.5;
                            goto mood_Snake;
                        }
                        RAND(good, result);
                    }
                    if (result.Food.Price == 0) MessageBox.Show("ERROR,No Food Is Randomed");
                    MW.GameSavesData.GameSave.Money -= result.Food.Price * 0.2;
                    TakeItem(result.Food);
                }
                else 
                {
                        result = null;
                        return;
                }
            }
            return;
        }
    }
}

