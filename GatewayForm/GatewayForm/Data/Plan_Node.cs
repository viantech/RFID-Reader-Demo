using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayForm
{
    
    public enum filter {EPC, User, TID}

    public class Plan_Node
    {
        public string name;
        public string anten_list;
        public bool protocol;
        public int weight;
        public Plan_Node()
        {
            

        }
    }
}
