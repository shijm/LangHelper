using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CompareDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (!File.Exists(textBox1.Text) || !File.Exists(textBox2.Text))
            //{
            //    return;
            //}
            //XElement xele1 = XElement.Load(textBox1.Text);
            //XElement xele2 = XElement.Load(textBox2.Text);
            //foreach (var item1 in xele1.Elements())
            //{
            //    var found = xele2.Elements().Where(o => o.Attribute("name").Value == item1.Attribute("name").Value);
            //    if (found != null && found.Count() > 0)
            //    {
            //        continue;
            //    }
            //    else
            //    {
            //        xele2.Add(item1);
            //    }
            //}
            //xele2.Save(textBox2.Text);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
                Compare();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = ofd.FileName;
                Compare();
            }
        }

        private void Compare()
        {
            if (!File.Exists(textBox1.Text) || !File.Exists(textBox2.Text))
            {
                return;
            }
            XElement xele1 = XElement.Load(textBox1.Text);
            XElement xele2 = XElement.Load(textBox2.Text);
            foreach (var item1 in xele1.Elements())
            {
                var found = xele2.Elements().Where(o => o.Attribute("name").Value == item1.Attribute("name").Value);
                if (found != null && found.Count() > 0)
                {
                    listBox2.Items.Add(new DisplayAndValue(item1, item1));
                    listBox1.Items.Add(new DisplayAndValue(item1, item1));
                }
                else
                {
                    listBox2.Items.Add(new DisplayAndValue(null, item1));
                    listBox1.Items.Add(new DisplayAndValue(item1, item1, true));
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            DisplayAndValue value = listBox1.SelectedItem as DisplayAndValue;
            int index = listBox1.SelectedIndex;
            if (value == null)
            {
                return;
            }
            DisplayAndValue value2 = listBox2.Items[index] as DisplayAndValue;
            value2.eleDisplay = value2.eleValue;
            listBox2.Items[index] = value2;
            listBox2.TopIndex = listBox1.TopIndex;
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            DisplayAndValue value = listBox2.SelectedItem as DisplayAndValue;
            int index = listBox2.SelectedIndex;
            if (value == null)
            {
                return;
            }
            value.eleDisplay = null;
            listBox2.Items[index] = value;
            listBox1.TopIndex = listBox2.TopIndex;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            XElement xele = XElement.Load(textBox2.Text);
            xele.Elements().Remove();
            foreach(var ele in listBox2.Items)
            {
                var displayandvalue = (ele as DisplayAndValue);
                if (displayandvalue.eleDisplay != null)
                {
                    xele.Add(displayandvalue.eleDisplay);
                }
            }
            xele.Save(textBox2.Text);
        }

        //private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    if(e.Index <0)
        //    {
        //        return;
        //    }
        //    DisplayAndValue displayandvalue = listBox1.Items[e.Index] as DisplayAndValue;
        //    if(displayandvalue == null || !displayandvalue.bState)
        //    {
        //        e.Graphics.DrawString(displayandvalue.eleDisplay.ToString(), e.Font, System.Drawing.Brushes.Black, e.Bounds);
        //        return;
        //    }
        //    e.Graphics.DrawString(displayandvalue.eleDisplay.ToString(), e.Font, System.Drawing.Brushes.Red, e.Bounds);
        //}
    }

    public class DisplayAndValue
    {
        public XElement eleDisplay;
        public XElement eleValue;
        public bool bState;

        public DisplayAndValue(XElement display, XElement value, bool state = false)
        {
            eleDisplay = display;
            eleValue = value;
            bState = state;
        }

        public override string ToString()
        {
            if(eleDisplay == null)
            {
                return "";
            }
            else
            {
                return eleDisplay.ToString();
            }
        }
    }
}
