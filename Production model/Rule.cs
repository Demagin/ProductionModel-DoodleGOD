using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Production_model
{
    public class Rule
    {
        public List<string> conditions;//ID условий
        public string results;//ID следствия

        public Rule(string r)
        {
            conditions = new List<string>();
            var temp = r.Split('-');
            results = temp[1].Trim(' ');
            var lst = temp[0].Split(',');
            foreach (var i in lst)
                conditions.Add(i.Trim(' '));
        }

        public bool compare(List<string> f)
        {
            bool res = true;
            foreach (var i in conditions)
                res = res && f.Contains(i);//проверяем есть ли все условия
            return res;
        }

        public string print()
        {
            Form1 _form = new Form1();
            string res = "";
            foreach (var i in conditions)
            {
                res += _form.facts[i];
                if (i != conditions.Last())
                    res += " , ";
            }
            res += "->" + _form.facts[results];
            return res;
        }
    }
}
