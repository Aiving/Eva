namespace Eva
{
    class Program
    {
        public static void Main() => new Bot().StartAsync().GetAwaiter().GetResult();
    }
}