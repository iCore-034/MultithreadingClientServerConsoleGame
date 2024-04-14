namespace GameData
{
    public class User : Block
    {    
        public override char Symbol
        {
            get => base.Symbol;
            set
            {
                if (value != ' ')
                {
                    base.Symbol = value;
                }
                else
                {
                    base.Symbol = '@';
                }
            }
        }
        public string Id {  get; set; }
    }
}
