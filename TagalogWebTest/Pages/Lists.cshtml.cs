using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TagalogWebTest.Pages
{
    public class Lists
    {
    }

    public class TagalogListItem
    {
        public string Tagalog { get; set; }
        public string English { get; set; }

        public TagalogListItem(){}
        public TagalogListItem(string tagalog, string english)
        {
            Tagalog = tagalog;
            English = english;
        }
    }


}
