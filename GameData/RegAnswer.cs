namespace GameData
{
    public class RegAnswer
    {
        public RegAnswerType type {  get; set; }
        public string message {  get; set; }      // ID или Сообщение об ошибке
        public enum RegAnswerType
        {
            DENY, ACCEPT
        }
    }
}
