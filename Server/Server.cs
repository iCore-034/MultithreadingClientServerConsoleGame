using System.Net;
using System.Net.Sockets;
using System.Text;
using GameData;
class Server
{
    public static List<RegForm>? reg_forms = new List<RegForm>();

    public static string anechkaDatabase = "AnechkaDatabase.json";

    public static List<User> Users = new List<User>();

    public static List<TcpClient> Clients = new List<TcpClient>();
    public static async Task<string> Receive(TcpClient client)
    {
        var stream = client.GetStream();
        var bytes = new List<byte>();

        int bytes_read = 0;

        while (true)
        {
            bytes_read = stream.ReadByte();

            bytes.Add((byte)bytes_read);

            if (bytes_read == '\0') break;
        }
        string info = Encoding.UTF8.GetString(bytes.ToArray());
        return info;
    }
    public static async Task SendMessage(TcpClient client, string info)
    {
        var bytes = Encoding.UTF8.GetBytes(info + "\0").ToArray();
        var stream = client.GetStream();
        await stream.WriteAsync(bytes.ToArray());
    }
    public static async Task ProcessRegistration(TcpClient client, RegForm? form, string info)
    {
        RegAnswer answer = new RegAnswer();
        // 0 - reg ,1  - auth

        form = Newtonsoft.Json.JsonConvert.DeserializeObject<RegForm>(info);
        if (reg_forms != null)
        {
            if (form.Status == RegForm.RegFormType.REG)
            {
                var new_form = reg_forms.Find(x => x.Login == form.Login);
                if (new_form == null)
                {
                    form.ID = Guid.NewGuid().ToString();
                    reg_forms.Add(form);
                    answer.type = RegAnswer.RegAnswerType.ACCEPT;
                    answer.message = form.ID;
                }
                else
                {
                    answer.type = RegAnswer.RegAnswerType.DENY;
                    answer.message = "Unavailable login";
                }
            }
            else if (form.Status == RegForm.RegFormType.AUTH)
            {
                var new_form = reg_forms.Find(x => x.Password == form.Password && x.Login == form.Login);
                if (new_form != null)
                {
                    answer.type = RegAnswer.RegAnswerType.ACCEPT;
                    answer.message = new_form.ID;
                }
                else
                {
                    answer.type = RegAnswer.RegAnswerType.DENY;
                    answer.message = "Wrong login or password";
                }
            }
        }
        string data = Newtonsoft.Json.JsonConvert.SerializeObject(answer);
        await SendMessage(client, data);
        data = Newtonsoft.Json.JsonConvert.SerializeObject(reg_forms);
        File.WriteAllText(anechkaDatabase, data);
        await SendMessage(client, form.ID);
    }
    public static async Task ProcessGame(TcpClient client, User user, string info)
    {
        if (user != null) user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(info);
        
        if (user != null)
        {
            if (user.X < 1 || user.X > 18 || user.Y < 1 || user.Y > 18)
            {
                user.X = 10;
                user.Y = 10;
            }
        }
        if (Users.Find(x => x.Id == user.Id) == null)
        {
            Users.Add(user);
            for (int i = 0; i < Users.Count; i++)
            {
                string userPtr = Newtonsoft.Json.JsonConvert.SerializeObject(Users[i]);
                await SendMessage(client, userPtr);
            }
        }
        else
        {
            // TODO: Поменять X Y у юзера в листе
            int i = Users.IndexOf(Users.Where(x => x.Id == user.Id).ElementAt(0));
            Users[i] = user;

        }
        string userStr = Newtonsoft.Json.JsonConvert.SerializeObject(user) + '\0';
        var bytes = Encoding.UTF8.GetBytes(userStr).ToArray();
        for (int i = 0; i < Clients.Count; i++)
        {
            if (Clients[i].Connected)
            {
                var send_message_stream = Clients[i].GetStream();
                await send_message_stream.WriteAsync(bytes);
            }
        }
    }
    public static async Task ProcessClient(TcpClient client)
    {
        try
        {
            while (true)
            {
                string info = await Receive(client);
                await Console.Out.WriteLineAsync(info);
                // REG FORM OR NOT
                RegForm? form = new RegForm();
                User? user = new User();

                if (info.Contains("Login") && info.Contains("Password"))
                {
                    await ProcessRegistration(client, form, info);
                }
                else if (info.Contains("X") && info.Contains("Y") && info.Contains("Symbol"))
                {
                    await ProcessGame(client, user, info);
                }

            }
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
            return;
        }
    }
    public static async Task Main(string[] args)
    {
        try
        {
            if (File.Exists(anechkaDatabase))
            {
                string file_base = File.ReadAllText(anechkaDatabase);
                reg_forms = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RegForm>>(file_base);
            }
            //TcpListener tcp_listener = new TcpListener(IPAddress.Parse("26.86.16.106"), 9010);
            TcpListener tcp_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9011);
            tcp_listener.Start();

            Console.WriteLine("Server started...\n");

            while (true)
            {
                TcpClient tcp_client = await tcp_listener.AcceptTcpClientAsync();

                await Console.Out.WriteLineAsync("\nClient started...\n");

                Clients.Add(tcp_client);

                _ = Task.Run(async () => await ProcessClient(Clients[Clients.Count - 1]));
            }
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
        }
        Console.ReadLine();
    }
}