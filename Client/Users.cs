using GameData;
public static  class Users
{
    public static List<User> list = new List<User>();
    public static void DeletePreviousPosition(User previous)
    {
        Console.SetCursorPosition(previous.X, previous.Y);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" ");
    }
    public static void SetCurrentPostion(User current)
    {
        Console.SetCursorPosition(current.X, current.Y);
        Console.ForegroundColor = current.Color;
        Console.Write(current.Symbol);
    }
}

