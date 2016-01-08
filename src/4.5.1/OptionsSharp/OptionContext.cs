﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.OptionsSharp
{
    public class OptionContext
    {
        private Option option;
        private string name;
        private int index;
        private OptionSet set;
        private OptionValueCollection c;

        public OptionContext(OptionSet set)
        {
            this.set = set;
            this.c = new OptionValueCollection(this);
        }

        public Option Option
        {
            get { return option; }
            set { option = value; }
        }

        public string OptionName
        {
            get { return name; }
            set { name = value; }
        }

        public int OptionIndex
        {
            get { return index; }
            set { index = value; }
        }

        public OptionSet OptionSet
        {
            get { return set; }
        }

        public OptionValueCollection OptionValues
        {
            get { return c; }
        }
    }
}
