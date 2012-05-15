using System.Collections.Generic;

namespace FreelancerModStudio.Data.IO
{
    public class StringTable
    {
        readonly Dictionary<int, string> _strings = new Dictionary<int, string>();

        public StringTable(string content)
        {
            if (content == null)
            {
                return;
            }

            int position = 0;
            foreach (string stringValue in content.Trim('\0').Split('\0'))
            {
                _strings.Add(position, stringValue);
                position += stringValue.Length + 1;
            }
        }

        public string GetString(int position)
        {
            return _strings[position];
        }
    }
}
