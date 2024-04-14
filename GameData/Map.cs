namespace GameData
{
    public static class Map
    {
        public static List<Block> field = new List<Block>();
        public static void Fill(int height, int width)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y == 0 || x == 0 || y == height - 1 || x == width - 1)
                    {
                        field.Add(new Block()
                        {
                            X = x,
                            Y = y,
                            Symbol = '#',
                            Color = ConsoleColor.Red,
                            status = Block.Status.BORDER
                        });
                    }
                    else
                    {
                        field.Add(new Block()
                        {
                            X = x,
                            Y = y,
                            Symbol = ' ',
                            Color = ConsoleColor.Black,
                            status = Block.Status.VOID
                        });
                    }
                }
            }
        }
        public static void Output()
        {
            for (int i = 0; i < field.Count; i++)
            {
                Console.SetCursorPosition(field[i].X, field[i].Y);
                Console.ForegroundColor = field[i].Color;
                Console.Write(field[i].Symbol);
            }
        }

    }
}
