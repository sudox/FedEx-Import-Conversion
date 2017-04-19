using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDImport
{
    [Serializable]
    public class HDSettings
    {
        public List<Model> models { get; set; }
        public string jobCode { get; set; }

        public HDSettings()
        {
            models = new List<Model>();
            jobCode = "";
        }
    }
}
