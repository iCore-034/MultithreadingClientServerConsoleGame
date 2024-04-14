using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using GameData;
class Client
{

    public static TcpClient client;
    public static async Task<bool> Send<T>(T me)
    {
        var stream = client.GetStream();
        string userJson = Newtonsoft.Json.JsonConvert.SerializeObject(me);
        userJson += '\0';
        byte[] bytes = Encoding.UTF8.GetBytes(userJson);

        await stream.WriteAsync(bytes);
        return true;
    }
    public static async Task<T> Receive<T>()
    {
        var stream = client.GetStream();
        while (true)
        {
            List<byte> bytes = new List<byte>();
            int bytes_read = 0;

            while ((bytes_read = stream.ReadByte()) != '\0')
            {
                bytes.Add((byte)bytes_read);
            }

            string userJson = Encoding.UTF8.GetString(bytes.ToArray());
            if (string.IsNullOrEmpty(userJson)) continue;
            T? answer = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(userJson);
            return answer;
        }
    }
    public static async Task GetMessage()
    {
        var stream = client.GetStream();
        List<byte> bytes = new List<byte>();

        while (true)
        {
            int bytes_read = 0;

            while ((bytes_read = stream.ReadByte()) != '\0')
            {
                bytes.Add((byte)bytes_read);
            }

            string userJson = Encoding.UTF8.GetString(bytes.ToArray());

            if (string.IsNullOrEmpty(userJson)) continue;

            User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(userJson);
            bool isContain = false;
            for (int i = 0; i < Users.list.Count; i++)
            {
                if (Users.list[i].Id == user.Id)
                {
                    Users.DeletePreviousPosition(Users.list[i]);
                    Users.list[i] = user;
                    Users.SetCurrentPostion(Users.list[i]);
                    isContain = true;
                    break;
                }
            }
            if (!isContain)
            {
                Users.list.Add(user);
                Users.SetCurrentPostion(user);
            }

            bytes.Clear();
        }
    }
    public static async Task SendMessage(User me)
    {
        int x = me.X, y = me.Y;
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.W:
                    me.Y -= 1;
                    if (!IsAvailable())
                    {
                        me.Y += 1;
                    }
                    break;
                case ConsoleKey.A:
                    me.X -= 1;
                    if (!IsAvailable())
                    {
                        me.X += 1;
                    }
                    break;
                case ConsoleKey.S:
                    me.Y += 1;
                    if (!IsAvailable())
                    {
                        me.Y -= 1;
                    }
                    break;
                case ConsoleKey.D:
                    me.X += 1;
                    if (!IsAvailable())
                    {
                        me.X -= 1;
                    }
                    break;
                default:
                    break;
            }
            if (x != me.X || y != me.Y)
            {
                await Send<User>(me);
            }
        }
        bool IsAvailable()
        {
            if (Map.field.Find(x => x.X == me.X && x.Y == me.Y).status != Block.Status.VOID)
            {
                return false;
            }
            if (Users.list.Find(x => x.X == me.X && x.Y == me.Y) != null)
            {
                return false;
            }
            return true;
        }
    }
    public static RegForm WriteLoginPassword()
    {
        try
        {
            Console.WriteLine("Registration [0] || Authentication [1]");

            ConsoleKeyInfo info = Console.ReadKey(true);

            RegForm loginPass = new RegForm();

            if (info.Key == ConsoleKey.D0) loginPass.Status = RegForm.RegFormType.REG;

            else if (info.Key == ConsoleKey.D1) loginPass.Status = RegForm.RegFormType.AUTH;

            Console.Write("Login:");

            string form = Console.ReadLine();

            if (form != null) { loginPass.Login = form; }

            Console.Write("Password:");

            form = Console.ReadLine();

            if (form != null)
            {

                var inputBytes = Encoding.UTF8.GetBytes(form);

                var inputHash = SHA256.HashData(inputBytes);

                loginPass.Password = Convert.ToHexString(inputHash);
            }
            return loginPass;
        }
        catch (Exception e) { return null; }
    }
    public static User InitUser()
    {
        Console.Write("Input symbol: ");
        for (int i = 1; i < 16; i++)
        {
            Console.ForegroundColor = (ConsoleColor)i;
            Console.Write(i + " ");
        }
        char symb = Console.ReadLine()[0];

        Console.Write("Choose color(1-15): ");
        ConsoleColor consoleColor = ConsoleColor.Green;
        int color = Convert.ToInt32(Console.ReadLine());
        if (!(color <= 0 || color >= 16))
        {
            consoleColor = (ConsoleColor)color;
        }
        Console.Clear();

        User user = new User()
        {
            X = 10,
            Y = 10,
            Color = consoleColor,
            Symbol = symb,
            Id = Guid.NewGuid().ToString(),
            status = Block.Status.PLAYER
        };
        return user;
    }
    public static async Task Main(string[] args)
    {
        client = new TcpClient();
        //await client.ConnectAsync(IPAddress.Parse("26.86.16.106"), 9011);
        await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 9011);

        RegAnswer answer = new RegAnswer() { type = RegAnswer.RegAnswerType.DENY};
        while (answer.type != RegAnswer.RegAnswerType.ACCEPT)
        {
            RegForm form = WriteLoginPassword();         // СОЗДАЛИ РЕГИСТРАЦИОННУЮ 
            await Send(form);                            // ОТПРАВИЛИ РЕГИСТРАЦИОННУЮ ФОРМУ
            answer = await Receive<RegAnswer>();

            if (answer == null) answer = new RegAnswer() { type = RegAnswer.RegAnswerType.DENY };
            else Console.WriteLine(answer.message);

            Thread.Sleep(1000);
            Console.Clear();
        }

        await Console.Out.WriteLineAsync($"ID: {answer.message}");
        User user = InitUser();                      // ИНИЦИАЛИЗАЦИЯ ИГРОВОГО ПЕРСОНАЖА
        user.Id = answer.message;

        Map.Fill(20, 20);
        Map.Output();
        Console.ResetColor();

        Users.list.Add(user);

        await Send<User>(user);

        _ = Task.Run(async () => await GetMessage());

        await SendMessage(Users.list.First(x => x.Id == user.Id));
    }
}