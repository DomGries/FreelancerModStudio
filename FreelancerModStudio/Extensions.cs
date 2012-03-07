
namespace FreelancerModStudio
{
    public static class Extensions
    {
        public static bool ContainsValue(this string[] values, string value)
        {
            foreach (var entry in values)
                if (entry.ToLower() == value)
                    return true;

            return false;
        }
    }
}
