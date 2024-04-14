using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData
{
    public class Block
    {
        public virtual char Symbol { get; set; }
        public ConsoleColor Color { get; set; }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public Status status { get; set; }
        public enum Status
        {
            BORDER, PLAYER, VOID 
        }
    }
}
