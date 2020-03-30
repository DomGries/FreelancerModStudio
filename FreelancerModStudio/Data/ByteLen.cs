namespace FreelancerModStudio.Data
{
    /// <summary>
    /// Contains the lenght in bytes of several types
    /// </summary>
    public static class ByteLen
    {
        public const int FileTag = 4;

        public const int Int16 = 2;
        public const int Int = 4;
        public const int Float = 4;

        public const int ConstantString = 64;
    }
}
