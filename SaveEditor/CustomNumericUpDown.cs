using System;
using System.Windows.Forms;

namespace SaveEditor
{
    public class CustomNumericUpDown : NumericUpDown
    {
        public new decimal Value
        {
            get => base.Value;
            set
            {
                if (value > base.Maximum) base.Value = base.Maximum;
                else if (value < base.Minimum) base.Value = base.Minimum;
                else base.Value = value;
            }
        }
        private void IncrementValue(decimal n)
        {
            var newval = this.Value + n;
            //if (newval >= this.Minimum && newval <= this.Maximum) this.Value = newval;
            if (newval > this.Maximum) this.Value = this.Maximum;
            else if (newval < this.Minimum) this.Value = this.Minimum;
            else this.Value = newval;
        }
        private short scrolledAmount = 0;
        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x20A;
            if (m.Msg != WM_MOUSEWHEEL)
                base.WndProc(ref m);
            else
            {
                var wparam = m.WParam.ToInt32();
                var hw = (short)((wparam & 0xffff0000) >> 16);

                scrolledAmount += hw;
                //Console.Write(scrolledAmount+" ");
                if (scrolledAmount <= -60)
                {
                    var val1 = (scrolledAmount - 60) / 120;
                    IncrementValue(val1 * base.Increment);
                    scrolledAmount -= (short)(val1 * 120);
                }
                else if (scrolledAmount >= 60)
                {
                    var val1 = (scrolledAmount + 60) / 120;
                    IncrementValue(val1 * base.Increment);
                    scrolledAmount -= (short)(val1 * 120);
                }
                //Console.WriteLine(scrolledAmount);
            }
        }
    }
}
