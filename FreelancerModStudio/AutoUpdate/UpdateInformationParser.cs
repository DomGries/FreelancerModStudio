using System;

namespace FreelancerModStudio.AutoUpdate
{
    internal static class UpdateInformationParser
    {
        public static UpdateInformation Parse(string content)
        {
            UpdateInformation updateInformation = new UpdateInformation();

            string[] line = content.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (line.Length < 3)
            {
                return null;
            }

            updateInformation.Version = new Version(line[0]);
            updateInformation.FileUri = new Uri(line[1].Trim());
            updateInformation.Silent = (line[2].Trim() == "1");

            return updateInformation;
        }
    }
}
