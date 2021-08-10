using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Production_model
{
    public partial class Form1 : Form
    {
        public Dictionary<string, string> facts = new Dictionary<string, string>();// ID факта, Факт
        public static Dictionary<string, Rule> rules = new Dictionary<string, Rule>();// ID правила, правило A => B
        int gameProgress = 4;

        //находим условия по следствию
        public static List<string> findRules(string id, List<string> rep)
        {
            List<string> result = new List<string>();
            foreach (var i in rules)
            {
                if (i.Value.results == id && !rep.Contains(i.Key))
                    result.Add(i.Key);
            }
            return result;
        }

        class Node
        {
            public string name;
            public List<Node> parents = new List<Node>();
            public List<Node> children = new List<Node>();
            public bool flag = false;
            public Node() { }
        }

        class RuleNode : Node
        {
           // public string name;
            public RuleNode() { }
            public RuleNode(string rule)
            {
                name = rule;
            }
        }

        class FactNode : Node
        {
           // public string name;
            public FactNode() { }
            public FactNode(string fact)
            {
                name = fact;
            }
        }

        public Form1()
        {
            InitializeComponent();
            facts = get_facts("..//..//facts.txt");
            rules = get_rules("..//..//rules.txt");
            load();
        }

        private void resolve(Node n)
        {
            if (n.flag)
                return;
            if (n is RuleNode)
            {
                n.flag = n.children.All(c => c.flag == true);//если есть все факты для правила, то ставим галочку
            }

            if (n is FactNode)
            {
                if (n.children.Count != 0)
                    n.flag = n.children.Any(c => c.flag == true);//если выполняется хотя бы одно правило, то ставим галочку
                else
                    n.flag = true;
            }

            //если получили галочку, то смотрим, что там у родителей
            if (n.flag)
                foreach (Node p in n.parents)
                    resolve(p);
        }

        public Tuple<bool, List<string>> backward_reasoning(List<string> Facts, string need_right)
        {

            List<string> known_facts = new List<string>(Facts);//входящие
            List<string> resId = new List<string>();//и нужный факт
            List<string> tmpRes = new List<string>();

            Dictionary<string, RuleNode> Rule_dict = new Dictionary<string, RuleNode>();
            Dictionary<string, FactNode> fact_dict = new Dictionary<string, FactNode>();
            FactNode root = new FactNode(need_right);
            fact_dict.Add(need_right, root);

            Stack<Node> tree = new Stack<Node>();
            tree.Push(root);

            while (tree.Count != 0)
            {
                Node current = tree.Pop();

                if (!known_facts.Contains(current.name))
                {
                    if (current is RuleNode)
                    {
                        RuleNode Rule_node = current as RuleNode;
                        foreach (var f in rules[Rule_node.name].conditions)
                            if (fact_dict.ContainsKey(f))//если факт ноды нет, то создаем и подвязываем, иначе, просто подвязываем
                            {
                                current.children.Add(fact_dict[f]);
                                fact_dict[f].parents.Add(current);
                            }
                            else
                            {
                                fact_dict.Add(f, new FactNode(f));
                                Rule_node.children.Add(fact_dict[f]);
                                fact_dict[f].parents.Add(Rule_node);
                                tree.Push(fact_dict[f]);
                            }
                    }
                    else
                    {
                        FactNode fact_node = current as FactNode;
                        foreach (var f in findRules(fact_node.name, facts.Keys.ToList()))
                            if (Rule_dict.ContainsKey(f))//если рул ноды нет, то создаем и подвязываем, иначе, просто подвязываем
                            {
                                current.children.Add(Rule_dict[f]);
                                Rule_dict[f].parents.Add(current);
                            }
                            else
                            {
                                Rule_dict.Add(f, new RuleNode(f));
                                fact_node.children.Add(Rule_dict[f]);
                                Rule_dict[f].parents.Add(fact_node);
                                tree.Push(Rule_dict[f]);
                            }
                    }
                }
            }

           foreach (var val in fact_dict)
           {
               if (known_facts.Contains(val.Key))//если факт есть то ставим ему галочку и считаем его родителей
               {
                   val.Value.flag = true;
                   foreach (Node p in val.Value.parents)
                       resolve(p);
                }
           }
           foreach (var val in fact_dict.Reverse())
           {
               if (known_facts.Contains(val.Key))
               {
                   foreach (var v in val.Value.parents)
                   {
                        if (!tmpRes.Contains(rules[v.name].results))//если не получали еще такого факта, то добавляем
                        {
                            var tmp = "";
                            tmp += facts[rules[v.name].conditions[0]] + " + " + facts[rules[v.name].conditions[1]] + " = " + facts[rules[v.name].results];
                            resId.Add(tmp);
                            known_facts.Add(rules[v.name].results);
                            tmpRes.Add(rules[v.name].results);
                        }
                   }
               }
           }
           if (root.flag == true)
           {
               resId.Add("Ура получился элемент : " + facts[root.name] + "!!!");
               if (root.name == need_right)
                   return Tuple.Create(true, resId);
           }
           return Tuple.Create(false, resId);
       }

       

        private void load()
        {
            foreach (var item in facts.Keys)
            {
                if (!checkBox1.Checked)
                {
                    FactsBox.Items.Add("" + item + ": " + facts[item]);
                    comboBox1.Items.Add("" + item + ": " + facts[item]);
                }
                else
                {
                    if (item.First() == 'S')
                    {
                        InFactsBox.Items.Add("" + item + ": " + facts[item]);
                    }
                    if (item.First() == 'F')
                    {   
                        FactsBox.Items.Add("" + item + ": " + facts[item]);
                        comboBox1.Items.Add("" + item + ": " + facts[item]);
                    }
                }
            }
            comboBox1.SelectedValue = "лава";
            comboBox1.SelectedItem = "лава";
        }

        private void loadGame()
        {
            foreach (var item in facts.Keys)
            {
                if (checkBox2.Checked)
                {
                    if (item.First() == 'S')
                    {
                        InFactsBox.Items.Add("" + item + ": " + facts[item]);
                        OutFactsBox.Items.Add("" + item + ": " + facts[item]);
                    }
                }
            }
        }

        private Dictionary<string, string> get_facts(string fname)
        {
            Dictionary<string, string> _facts = new Dictionary<string, string>();
            using (StreamReader fs = new StreamReader(fname))
            {
                while (true)
                {
                    string temp = fs.ReadLine();
                    if (temp == null) break;
                    string[] strs = temp.Split(':');
                    _facts.Add(strs[0].Trim(' '), strs[1]);
                }
            }
            return _facts;
        }

        private Dictionary<string, Rule> get_rules(string fname)
        {
            Dictionary<string, Rule> _rules = new Dictionary<string, Rule>();
            using (StreamReader fs = new StreamReader(fname))
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    var temp = line.Split(':');
                    temp[1] = temp[1].Trim(' ');
                    _rules[temp[0]] = new Rule(temp[1]);
                }
            }
            return _rules;
        }

        private void agenda(Dictionary<string, Rule> _rules, ref List<string> f, ref bool b)
        {
            b = false;//если ничего не получим, то отсановимся
            foreach (var i in _rules)
                if (i.Value.compare(f))
                {
                    var res = i.Value.results;
                    if (!f.Contains(res))
                    {
                        f.Add(i.Value.results);
                        b = true;
                        ResultBox.Text += i.Value.print() + Environment.NewLine;
                        OutFactsBox.Items.Add(facts[i.Value.results]);
                    }
                }
        }
        private Tuple<bool, List<string>> ret_agenda()
        {
            List<string> list = new List<string>();
            string need_id = comboBox1.SelectedItem.ToString().Split(':')[0].Trim(' ');

            List<string> first_facts = new List<string>();
            foreach (var i in InFactsBox.Items)
                first_facts.Add(i.ToString().Split(':')[0].Trim(' '));
            var res = backward_reasoning(first_facts, need_id);
            return res;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        //старт
        private void button2_Click(object sender, EventArgs e)
        {
            ResultBox.Text = "";
            List<string> in_fact = new List<string>();
            foreach (var i in InFactsBox.Items)
                in_fact.Add(i.ToString().Split(':')[0].Trim(' '));//ID выбранных фактов, отсюда берем
            if (!checkBox1.Checked)
            {
                Dictionary<string, Rule> _rules = rules;// ID правила, правило A => B // сюда выгружаем
                OutFactsBox.Items.Clear();
                bool b = true;
                while (b) { agenda(rules, ref in_fact, ref b); }
            }
            else
            {
                var r = ret_agenda();
                ResultBox.Text = "Получился ли элемент? " + (r.Item1 ? "Да" : "Нет, магии не произошло");

                if (r.Item1)
                    foreach (var id in r.Item2)
                        ResultBox.Text += Environment.NewLine + id;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        //сбросить
        private void button1_Click(object sender, EventArgs e)
        {
            InFactsBox.Items.Clear();
            FactsBox.Items.Clear();
            OutFactsBox.Items.Clear();
            ResultBox.Text = "";
            gameProgress = 4;
            label7.Text = "Полученно элементов: 4 из 126";
            textBox1.Text = "Попробуй что-нибудь объеденить!";
            FirstElem.Items.Clear();
            SecondElem.Items.Clear();
            InFactsBox.Enabled = true;
            OutFactsBox.Enabled = true;
            pictureBox3.Visible = false;
            if (checkBox2.Checked)
                loadGame();
            else
                load();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label5.Text = "Выберите финальный факт";
                label4.Text = "Обратный вывод";
                OutFactsBox.Visible = false;
                comboBox1.Visible = true;
            }
            else
            {
                label5.Text = "Вжух, получили элементы!";
                label4.Text = "Прямой вывод";
                OutFactsBox.Visible = true;
                comboBox1.Visible = false;
            }
            InFactsBox.Items.Clear();
            FactsBox.Items.Clear();
            OutFactsBox.Items.Clear();
            ResultBox.Text = "";
            load();
        }

        private void FactsBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void InFactsBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OutFactsBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ResultBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void FactsBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (FactsBox.SelectedItem != null)
            {
                InFactsBox.Items.Add(FactsBox.SelectedItem);
                FactsBox.Items.Remove(FactsBox.SelectedItem);
            }

        }

        private void InFactsBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (InFactsBox.SelectedItem != null)   
            if (checkBox2.Checked)
            {
                FirstElem.Items.Add(InFactsBox.SelectedItem);
                InFactsBox.Items.Remove(InFactsBox.SelectedItem);
                InFactsBox.Enabled = false;
            }
            else
            {
                FactsBox.Items.Add(InFactsBox.SelectedItem);
                InFactsBox.Items.Remove(InFactsBox.SelectedItem);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_Enter(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                label3.Text = "Выберите первый элемент";
                label5.Text = "Выберите второй элемент";
                label4.Text = "Посмотрим, что у нас получилось.";

                // OutFactsBox.Visible = false;
                //comboBox1.Visible = true;
                InFactsBox.Items.Clear();
                FactsBox.Items.Clear();
                OutFactsBox.Items.Clear();
                ResultBox.Text = "";
                FactsBox.Visible = false;
                ResultBox.Visible = false;
                checkBox1.Visible = false;
                comboBox1.Visible = false;
                start.Visible = false;
                textBox1.Visible = true;
                button1.Visible = true;
                label6.Visible = true;
                label7.Visible = true;
                pictureBox2.Visible = true;
                RulesBox.Visible = true;
                FirstElem.Visible = true;
                SecondElem.Visible = true;
                InFactsBox.Enabled = true;
                OutFactsBox.Enabled = true;
                loadGame();
            }
            else
            {
                button1_Click(sender, e);
                label3.Text = "Выбранные элементы";
                FactsBox.Visible = true;
                ResultBox.Visible = true;
                checkBox1.Visible = true;
                start.Visible = true;
                textBox1.Visible = false;
                button1.Visible = false;
                label6.Visible = false;
                pictureBox2.Visible = false;
                RulesBox.Visible = false;
                FirstElem.Visible = false;
                SecondElem.Visible = false;
                label7.Visible = false;
                pictureBox3.Visible = false;
                checkBox1_CheckedChanged(sender, e);
            }
           
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void OutFactsBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (OutFactsBox.SelectedItem != null)
            {
                if (checkBox2.Checked)
                {
                    SecondElem.Items.Add(OutFactsBox.SelectedItem);
                    OutFactsBox.Items.Remove(OutFactsBox.SelectedItem);
                    OutFactsBox.Enabled = false;
                }
            }
        }

        private void FirstElem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (FirstElem.SelectedItem != null)
            {
                InFactsBox.Items.Add(FirstElem.SelectedItem);
                FirstElem.Items.Remove(FirstElem.SelectedItem);
                InFactsBox.Enabled = true;
            }
        }

        private void SecondElem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SecondElem.SelectedItem != null)
            {
                OutFactsBox.Items.Add(SecondElem.SelectedItem);
                SecondElem.Items.Remove(SecondElem.SelectedItem);
                OutFactsBox.Enabled = true;
            }
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
            var tmp = "Ничего не произошло! Попробуй другие элементы!";
            if (FirstElem.Items.Count > 0 && SecondElem.Items.Count > 0)
            {
                List<string> in_fact = new List<string>();
                in_fact.Add(FirstElem.Items[0].ToString().Split(':')[0].Trim(' '));
                in_fact.Add(SecondElem.Items[0].ToString().Split(':')[0].Trim(' '));
                foreach (var i in rules)
                    if (i.Value.compare(in_fact))
                    {
                        var res = i.Value.results;
                        if (!InFactsBox.Items.Contains(res + ": " + facts[res]) && !OutFactsBox.Items.Contains(res + ": " + facts[res]))
                        {
                            in_fact.Add(i.Value.results);
                            gameProgress++;
                            tmp = "Ура, новый элемент!" + Environment.NewLine + i.Value.print() + Environment.NewLine;
                            InFactsBox.Items.Add(res + ": " + facts[res]);
                            OutFactsBox.Items.Add(res + ": " + facts[res]);
                            label7.Text = "Полученно элементов: " + gameProgress + " из 126";
                            if (gameProgress == 126)
                            {
                                tmp += "Поздравляем! Ты получил все элементы!";
                                pictureBox3.Visible = true;
                            }
                            break;
                        }
                        else
                        {
                            tmp = "У нас уже есть такой элемент! Попробуй что-то новое! Например Лягушку и Энергию :)";

                            break;
                        }
                    }
            }
            else
                tmp = "Выбери оба элемента!";
            textBox1.Text = tmp;
        }
    }
}
