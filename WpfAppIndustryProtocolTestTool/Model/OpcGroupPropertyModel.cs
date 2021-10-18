using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.Model
{
    public class OpcGroupPropertyModel
    {
        public string GroupName { get; set; }
        public float DefaultGroupDeadband { get; set; }
        public int UpdateRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubscribed { get; set; }
    }
}
