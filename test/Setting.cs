using LinePutScript;
using LinePutScript.Converter;

namespace VPET.Evian.TEST
{
    public class Setting : Line
    {
        public Setting(ILine line) : base(line)
        {
        }
        public Setting()
        {
        }
        /// <summary>
        /// 最大购买金额
        /// </summary>
        [Line]
        public int MaxPrice
        {
            get => maxprice; set
            {
                maxprice = value;
                MaxPriceStr = $"{value:f2}";
            }
        }
        private int maxprice = 100;
        public string MaxPriceStr { get; private set; } = "100";
        /// <summary>
        /// 状态-最小口渴度
        /// </summary>
        [Line]
        public int MinThirst
        {
            get => minthirst; set
            {
                minthirst = value;
                MinThirstStr = $"{value:f2}%";
            }
        }
        private int minthirst = 80;
        public string MinThirstStr { get; private set; } = "80%";
        /// <summary>
        /// 状态-最小饱腹度
        /// </summary>
        [Line]
        public int MinSatiety
        {
            get => minsatiety; set
            {
                minsatiety = value;
                MinSatietyStr = $"{value:f2}%";
            }
        }
        private int minsatiety = 80;
        public string MinSatietyStr { get; private set; } = "80%";
        /// <summary>
        /// 状态-最小心情值
        /// </summary>
        [Line]
        public int MinMood
        {
            get => minmood; set
            {
                minmood = value;
                MinMoodStr = $"{value:f2}%";
            }
        }
        private int minmood = 80;
        public string MinMoodStr { get; private set; } = "80%";
        /// <summary>
        /// 状态-最小健康值
        /// </summary>
        [Line]
        public int MinHealth
        {
            get => minhealth; set
            {
                minhealth = value;
                MinHealthStr = $"{value:f2}%";
            }
        }
        private int minhealth = 90;
        public string MinHealthStr { get; private set; } = "90%";
        /// <summary>
        /// 启用条件-最低存款
        /// </summary>
        [Line]
        public int MinDeposit
        {
            get => mindeposit; set
            {
                mindeposit = value;
                MinDepositStr = $"{value:f2}";
            }
        }
        private int mindeposit = 100;
        public string MinDepositStr { get; private set; } = "100";
        /// <summary>
        /// 属性-最小口渴度
        /// </summary>
        [Line]
        public int MinGoodThirst
        {
            get => mingoodthirst; set
            {
                mingoodthirst = value;
                MinGoodThirstStr = $"{value:f2}%";
            }
        }
        private int mingoodthirst = 5;
        public string MinGoodThirstStr { get; private set; } = "5%";
        /// <summary>
        /// 属性-最小饱腹度
        /// </summary>
        [Line]
        public int MinGoodSatiety
        {
            get => mingoodsatiety; set
            {
                mingoodsatiety = value;
                MinGoodSatietyStr = $"{value:f2}%";
            }
        }
        private int mingoodsatiety = 5;
        public string MinGoodSatietyStr { get; private set; } = "5%";
        /// <summary>
        /// 属性-最小心情值
        /// </summary>
        [Line]
        public int MinGoodMood
        {
            get => mingoodmood; set
            {
                mingoodmood = value;
                MinGoodMoodStr = $"{value:f2}%";
            }
        }
        private int mingoodmood = 5;
        public string MinGoodMoodStr { get; private set; } = "5%";
        /// <summary>
        /// 属性-最小健康值
        /// </summary>
        [Line]
        public int MinGoodHealth
        {
            get => mingoodhealth; set
            {
                mingoodhealth = value;
                MinGoodHealthStr = $"{value:f2}%";
            }
        }
        private int mingoodhealth = 5;
        public string MinGoodHealthStr { get; private set; } = "5%";
        /// <summary>
        /// 启用SettingPP
        /// </summary>
        [Line]
        public bool Enable { get; set; } = true;
    }
}
