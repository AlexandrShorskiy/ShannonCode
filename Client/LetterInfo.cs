using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Part : IEquatable<Part>, IComparable<Part>
    {
        public char PartName { get; set; }

        public double PartCount { get; set; }

        public override string ToString()
        {
            return PartName + "\t" + PartCount + "\n";
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Part objAsPart = obj as Part;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public int SortByNameAscending(string name1, string name2)
        {
            return name1.CompareTo(name2);
        }
        public int CompareTo(Part comparePart)
        {
            if (comparePart == null)
                return 1;

            else
                return this.PartCount.CompareTo(comparePart.PartCount);
        }

        public bool Equals(Part other)
        {
            if (other == null) return false;
            return (this.PartCount.Equals(other.PartCount));
        }
    }
}
