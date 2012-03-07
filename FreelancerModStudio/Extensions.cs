
namespace FreelancerModStudio
{
    public static class Extensions
    {
        public static bool ContainsValue(this string[] array, string value)
        {
            foreach (var entry in array)
                if (entry == value)
                    return true;

            return false;
        }
    }
}
