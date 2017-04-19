using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDImport
{
    [Serializable]
    public class Model
    {
        public string code;
        public List<string> items;
        public string weight;
        public string length;
        public string width;
        public string height;
        public string oversize;

        public Model()
        {

        }

        public Model(string c, List<string> i, string w, string l, string wi, string h, bool o)
        {
            code = c;
            items = i;
            weight = w;
            length = l;
            width = wi;
            height = h;
            if (o)
            {
                oversize = "Y";
            }
            oversize = "N";
        }

        public Model(string[] fileIn)
        {
            List<string> line = fileIn.ToList<string>();

            code = line[0];
            line.RemoveAt(0);

            oversize = line[line.Count - 1];
            line.RemoveAt(line.Count - 1);

            height = line[line.Count - 1];
            line.RemoveAt(line.Count - 1);

            width = line[line.Count - 1];
            line.RemoveAt(line.Count - 1);

            length = line[line.Count - 1];
            line.RemoveAt(line.Count - 1);

            weight = line[line.Count - 1];
            line.RemoveAt(line.Count - 1);

            items = line;
        }

        public string getCode()
        {
            return code;
        }

        public string getWeight()
        {
            return weight;
        }

        public string getLength()
        {
            return length;
        }

        public string getWidth()
        {
            return width;
        }

        public string getHeight()
        {
            return height;
        }

        public string getOversize()
        {
            return oversize;
        }

        public List<string> getItems()
        {
            return items;
        }

        public List<string> getItemLine(string refCode)
        {
            var outLine = new List<string>();
            outLine.Add(weight);
            outLine.Add(length);
            outLine.Add(width);
            outLine.Add(height);
            outLine.Add(oversize);
            outLine.Add(refCode + " " + code);
            return outLine;
        }

        public bool compare(List<string> orderLine)
        {
            // Determine if each element is in the items array
            return false;
        }

        public int count()
        {
            return items.Count;
        }

        public bool contains(List<string> contents)
        {
            try
            {
                foreach (var i in items)
                {
                    if (contents.IndexOf(i) == -1)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                return false;
            }
        }

        public override string ToString()
        {
            return getCode();
        }
    }
}
