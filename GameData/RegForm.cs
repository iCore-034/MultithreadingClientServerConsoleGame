namespace GameData
{
    public class RegForm
    {
        public string ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public RegFormType Status { get; set; }
        public enum RegFormType
        {
            REG, AUTH
        }
    }
}
