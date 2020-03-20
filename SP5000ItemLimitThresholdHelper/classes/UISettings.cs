using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP5000ItemLimitThresholdHelper.classes
{
    public class UISettings
    {
        public string SiteURL;
        public string Source;
        public string Dest;
        public bool MCOverwrite;
        public bool Simulate;
        public List<int> IdsIncl;
        public List<int> IdsExcl;
        public string UrlsIncl;
        public string UrlsExcl;
    }
    public class ListDataField
    {
        public string DisplayName { get; set; }
        public string InternalName { get; set; }
        public string FieldType { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
