using System;
using FreelancerModStudio.SystemDesigner.Content;

namespace FreelancerModStudio.Data
{
    [Serializable]
    public class ArchetypeInfo
    {
        public ContentType Type { get; set; }
        public double Radius { get; set; }
        public string ModelPath { get; set; }

        public override string ToString()
        {
            return Type + ", " + Radius;
        }
    }
}
