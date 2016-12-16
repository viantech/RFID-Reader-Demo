using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayForm
{
    public enum FILTER { EPC, User, TID }
    class Plan_Node
    {
        public class Plan_Root
        {
            public List<Plan_Struct> plan_list { get; set; }
            //public List<string> list_plan_name { get; set; }
            public Plan_Root()
            {
                plan_list = new List<Plan_Struct>();
                //list_plan_name = new List<string>();
            }
            /*public string DefaultNewPlanName()
            {
                if (list_plan_name.Count == 0)
                {
                    list_plan_name.Add("Plan1");
                    return "Plan1";
                }
                else
                {
                    for (int idx = 0; idx < list_plan_name.Count + 1; idx++)
                    {
                        if (!list_plan_name.Contains("Plan" + (idx + 1).ToString()))
                        {
                            list_plan_name.Add("Plan" + (idx + 1).ToString());
                            break;
                        }
                    }
                    return list_plan_name[list_plan_name.Count - 1];
                }
            }
            public void Load(string plan_str)
            {
                string[] seperators = new string[] { "SimpleReadPlan:[Antennas=[", ",Protocol=", ",Filter=", ",Op=", ",UseFastSearch=", ",Weight=" };
                string[] field = plan_str.Split(seperators, StringSplitOptions.None);
                for (int num_plan = 0; num_plan < field.Length / 6; num_plan++)
                {
                    Plan_Struct theplan = new Plan_Struct();
                    theplan.name = New_NamePlan();
                    theplan.antena = field[6 * num_plan + 1].TrimEnd(']');
                    theplan.type = FILTER.EPC;
                    if (theplan.type == FILTER.EPC)
                    {
                        theplan.EPC = field[6 * num_plan + 3].Substring(field[6 * num_plan + 3].IndexOf('=') + 1).TrimEnd(']');
                    }
                    theplan.weight = field[6 * num_plan + 6].Substring(0, field[6 * num_plan + 6].Length - 2);
                    plans_list.Add(theplan);
                }
            }*/
        }
        
        public class Plan_Struct
        {
            public string name;
            public string antena;
            public FILTER type;
            public string EPC;
            public string weight;

            public Plan_Struct(string name , string antena = "1", FILTER filter_type = FILTER.EPC, string EPC = "AAAA", string weight = "1")
            {
                this.name = name;
                this.antena = antena; 
                this.type = filter_type;
                this.EPC = EPC;
                this.weight = weight;
            }
        }
    }
}
