using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;


namespace VPET.Evian.TEST
{
    public class SettingPP : MainPlugin
    {
        public Setting Set;

        public override string PluginName => "SettingPP";
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public SettingPP(IMainWindow mainwin) : base(mainwin)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
        }
        public override void LoadPlugin()
        {
            ///从Setting.lps中读取存储的设置
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
            ///将自动购买功能挂在SpendHandle上
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
            if (MW.Set.AutoBuy && Set.Enable) 
            {
                MW.Set.AutoBuy = false;
                MessageBoxX.Show("桌宠自带的自动购买和本mod的自动购买冲突，已关闭桌宠自带的自动购买".Translate(), "错误".Translate(),MessageBoxButton.OK, MessageBoxIcon.Error,DefaultButton.YesOK,5);
            }
            var sm = MW.GameSavesData.GameSave.StrengthMax;
            var smood = MW.GameSavesData.GameSave.FeelingMax;
            if (Set.Enable && MW.GameSavesData.GameSave.Money >= (Set.MinDeposit > 100 ? Set.MinDeposit : 100))
            {
                if(Set.Mode == false)
                {
                    Autobuy_OFFViolence(sm, smood);
                }
                else
                {
                    Autobuy_OnViolence(sm, smood);
                }
                
            }
            return;
        }
        private void Autobuy_OnViolence(double sm,double smood) 
        {
            if ((MW.GameSavesData.GameSave.StrengthFood + MW.GameSavesData.GameSave.StoreStrengthFood) < sm * Set.MinSatiety * 0.01)
            {
                var addS = (Set.MinSatiety + 5) * sm * 0.01 - MW.GameSavesData.GameSave.StrengthFood - MW.GameSavesData.GameSave.StoreStrengthFood;
                var delMoney = addS / 6;
                if(MW.GameSavesData.GameSave.Money>= delMoney)
                {
                    addS += MW.GameSavesData.GameSave.StoreStrengthFood;
                    MW.GameSavesData.GameSave.StoreStrengthFood = 0;
                    MW.GameSavesData.GameSave.Money -= delMoney;
                    MW.GameSavesData.GameSave.StrengthFood += addS;
                }
            }
            else if ((MW.GameSavesData.GameSave.StrengthDrink + MW.GameSavesData.GameSave.StoreStrengthDrink) < sm * Set.MinThirst * 0.01)
            {
                var addD = (Set.MinThirst + 5) * sm * 0.01 - MW.GameSavesData.GameSave.StrengthDrink - MW.GameSavesData.GameSave.StoreStrengthDrink;
                var delMoney = addD / 9;
                if (MW.GameSavesData.GameSave.Money >= delMoney)
                {
                    addD += MW.GameSavesData.GameSave.StoreStrengthDrink;
                    MW.GameSavesData.GameSave.StoreStrengthDrink = 0;
                    MW.GameSavesData.GameSave.Money -= delMoney;
                    MW.GameSavesData.GameSave.StrengthDrink += addD;
                }
            }
            else if (MW.GameSavesData.GameSave.Health < 100 * Set.MinHealth * 0.01)
            {
                var addH= (Set.MinHealth + 5) * sm *0.01-MW.GameSavesData.GameSave.Health;
                var delMoney = addH;
                if (MW.GameSavesData.GameSave.Money >= delMoney)
                {
                    MW.GameSavesData.GameSave.Money -= delMoney;
                    MW.GameSavesData.GameSave.Health += addH;
                }
            }
            else if (MW.GameSavesData.GameSave.Feeling + MW.GameSavesData.GameSave.StoreFeeling < smood * Set.MinMood * 0.01)
            {
                var addF= (Set.MinMood + 5) * sm * 0.01 - MW.GameSavesData.GameSave.Feeling - MW.GameSavesData.GameSave.StoreFeeling;
                var delMoney = addF / 15;
                if (MW.GameSavesData.GameSave.Money >= delMoney)
                {
                    addF += MW.GameSavesData.GameSave.StoreFeeling;
                    MW.GameSavesData.GameSave.StoreFeeling = 0;
                    MW.GameSavesData.GameSave.Money -= delMoney;
                    MW.GameSavesData.GameSave.Feeling += addF;
                }
            }
        }
        private Food Double(Food food,bool gift = false)
        {
            var sm = MW.GameSavesData.GameSave.StrengthMax;
            var smood = MW.GameSavesData.GameSave.FeelingMax;

            var FM = sm * Set.MinSatiety * 0.01 - MW.GameSavesData.GameSave.StrengthFood - MW.GameSavesData.GameSave.StoreStrengthFood;
            var DM = sm * Set.MinThirst * 0.01 - MW.GameSavesData.GameSave.StrengthDrink - MW.GameSavesData.GameSave.StoreStrengthDrink;
            var MM = smood * Set.MinMood * 0.01 - MW.GameSavesData.GameSave.Feeling - MW.GameSavesData.GameSave.StoreFeeling;
            var Ratio_relS = MW.GameSavesData.GameSave.StrengthMax / 100;
            var Ratio_relF = MW.GameSavesData.GameSave.FeelingMax / 100;
            if (food.Type == Food.FoodType.Meal)
            {
                if (food.StrengthFood * Ratio_relS > FM)
                {
                    if (food.StrengthFood / 2 < FM && food.StrengthFood * Ratio_relS < 2 * FM)
                        return food;
                    else
                    {
                        food.Price -= (food.StrengthFood - FM - 0.1 * sm) / 6 * 0.7;
                        food.StrengthFood = FM + 0.1 * sm;
                    }
                }
                else
                {
                    food.Price += food.StrengthFood * (Ratio_relS - 1) / 6 * 0.7;
                    food.StrengthFood *= Ratio_relS;
                }
            }
            else if (food.Type == Food.FoodType.Drink)
            {
                if (food.StrengthDrink * Ratio_relS > DM)
                {
                    if (food.StrengthDrink / 2 < DM && food.StrengthDrink * Ratio_relS < 2 * DM)
                        return food;
                    else
                    {
                        food.Price -= (food.StrengthDrink - DM - 0.1 * sm) / 6 * 0.7;
                        food.StrengthDrink = DM + 0.1 * sm;
                    }
                }
                else
                {
                    food.Price += food.StrengthDrink * (Ratio_relS - 1) / 9 * 0.7;
                    food.StrengthDrink *= Ratio_relS;
                }
            }
            else if (food.Type == Food.FoodType.Snack)
            {
                if(!gift)
                {
                    if (food.StrengthFood * Ratio_relS > FM)
                    {
                        if (food.StrengthFood / 2 < FM && food.StrengthFood * Ratio_relS < 2 * FM) ;
                        else
                        {
                            food.Price -= (food.StrengthFood - FM - 0.1 * sm) / 6 * 0.7;
                            food.StrengthFood = FM + 0.1 * sm;
                        }
                    }
                    else
                    {
                        food.Price += food.StrengthFood * (Ratio_relS - 1) / 6 * 0.7;
                        food.StrengthFood *= Ratio_relS;
                    }
                }
                if(gift)
                {
                    if (food.Feeling * Ratio_relF > MM)
                    {
                        if (food.Feeling / 2 < MM && food.Feeling * Ratio_relF < 2 * MM) ;
                        else
                        {
                            food.Price -= (food.Feeling - MM - 0.1 * smood) / 15 * 0.7;
                            food.Feeling = MM + 0.1 * smood;
                        }
                    }
                    else
                    {
                        food.Price += food.Feeling * (Ratio_relF - 1) / 15 * 0.7;
                        food.Feeling *= Ratio_relF;
                    }
                }
            }
            else if (food.Type == Food.FoodType.Gift)
            {
                if (food.Feeling * Ratio_relF > MM)
                {
                    if (food.Feeling / 2 < MM && food.Feeling * Ratio_relF < 2 * MM)
                        return food;
                    else
                    {
                        food.Price -= (food.Feeling - MM - 0.1 * smood) / 15 * 0.7;
                        food.Feeling = MM + 0.1 * smood;
                    }
                }
                else
                {
                    food.Price += food.Feeling * (Ratio_relF - 1) / 15 * 0.7;
                    food.Feeling *= Ratio_relF;
                }
            }
            return food;
        }
        private void Autobuy_OFFViolence(double sm, double smood)
        {
            var havemoney = (Set.MaxPrice < MW.GameSavesData.GameSave.Money ? Set.MaxPrice : MW.GameSavesData.GameSave.Money);
            List<Food> food = MW.Foods.FindAll(x => x.Price >= 2 && x.Health >= 0 && x.Exp >= 0 && x.Likability >= 0 && x.Price < havemoney //桌宠不吃负面的食物
             && !x.IsOverLoad() // 不吃超模食物
             );
            if ((MW.GameSavesData.GameSave.StrengthFood + MW.GameSavesData.GameSave.StoreStrengthFood) < sm * Set.MinSatiety * 0.01)
            {
                if ((MW.GameSavesData.GameSave.StrengthFood + MW.GameSavesData.GameSave.StoreStrengthFood) < sm * Set.MinSatiety * 0.01 * 0.8)
                {//太饿了,找正餐
                    food = food.FindAll(x => x.Type == Food.FoodType.Meal && x.StrengthFood > (Set.MinGoodSatiety < 100 ? Set.MinGoodSatiety : 100));
                }
                else
                {//找零食
                    food = food.FindAll(x => x.Type == Food.FoodType.Snack && x.StrengthFood > (Set.MinGoodSatiety < 100 ? Set.MinGoodSatiety : 100));
                }
                if (food.Count == 0)
                    return;
                var item = food[Function.Rnd.Next(food.Count)];
                item = Double(item);
                MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                TakeItem(item);
            }
            else if ((MW.GameSavesData.GameSave.StrengthDrink + MW.GameSavesData.GameSave.StoreStrengthDrink) < sm * Set.MinThirst * 0.01)
            {
                food = food.FindAll(x => x.Type == Food.FoodType.Drink && x.StrengthDrink > (Set.MinGoodThirst < 100 ? Set.MinGoodThirst : 100));
                if (food.Count == 0)
                    return;
                var item = food[Function.Rnd.Next(food.Count)];
                item = Double(item);
                MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                TakeItem(item);
            }
            else if (MW.GameSavesData.GameSave.Health < 100 * Set.MinHealth * 0.01)
            {
                food = food.FindAll(x => x.Type == Food.FoodType.Drug && x.Health > (Set.MinGoodHealth < 100 ? Set.MinGoodHealth : 100));
                if (food.Count == 0)
                    return;
                var item = food[Function.Rnd.Next(food.Count)];
                MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                TakeItem(item);
            }
            else if ((MW.GameSavesData.GameSave.Feeling + MW.GameSavesData.GameSave.StoreFeeling) < smood * Set.MinMood * 0.01)
            {
                if ((MW.GameSavesData.GameSave.Feeling + MW.GameSavesData.GameSave.StoreFeeling) < smood * Set.MinMood * 0.01 * 0.8)
                {
                mood_Gift:
                    food = food.FindAll(x => x.Type == Food.FoodType.Gift && x.Feeling > (Set.MinGoodMood < 100 ? Set.MinGoodMood : 100));
                    if (food.Count == 0)
                    {
                        smood *= 0.5;
                        goto mood_Gift;
                    }
                    var item = food[Function.Rnd.Next(food.Count)];
                    item = Double(item);
                    MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                    TakeItem(item);
                }
                else
                {
                mood_Snake:
                    food = food.FindAll(x => x.Type == Food.FoodType.Gift && x.Feeling > (Set.MinGoodMood < 100 ? Set.MinGoodMood : 100));
                    if (food.Count == 0)
                    {
                        smood *= 0.8;
                        goto mood_Snake;
                    }
                    var item = food[Function.Rnd.Next(food.Count)];
                    item = Double(item,true);
                    MW.GameSavesData.GameSave.Money -= item.Price * 0.2;
                    TakeItem(item);
                }
            }
            
        }
    }
}

