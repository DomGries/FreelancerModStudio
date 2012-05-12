using System;
using FreelancerModStudio.SystemPresenter.Content;

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
            return Type.ToString() + ", " + Radius.ToString();
        }
    }
}