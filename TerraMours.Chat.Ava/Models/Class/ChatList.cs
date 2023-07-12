using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Models.Class {
    public class ChatList {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
    }
}
