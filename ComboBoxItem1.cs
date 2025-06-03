using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Helpers
{
    public class ComboBoxItem1
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public ComboBoxItem1(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;  // This is what will be displayed in the ComboBox
        }
    }

}
