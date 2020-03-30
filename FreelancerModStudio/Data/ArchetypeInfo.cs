namespace FreelancerModStudio.Data
{
    using System;

    using FreelancerModStudio.SystemDesigner.Content;

    [Serializable]
    public class ArchetypeInfo
    {
        public ContentType Type { get; set; }
        public double Radius { get; set; }
        public string ModelPath { get; set; }

        public override string ToString()
        {
            return this.Type + ", " + this.Radius;
        }
    }
}
