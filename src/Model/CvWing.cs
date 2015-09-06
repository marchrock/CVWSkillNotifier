using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;

namespace CvwSkillNotifier.Model
{
    class CvWing
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = "九七式艦攻+七一式電探（蓮見隊）";
        public int SkillLevel { get; set; } = -1;
        public int PreviousSkillLevel { get; set; } = -1;
        public SlotItem RowData { get; set; } = null;
    }
}
