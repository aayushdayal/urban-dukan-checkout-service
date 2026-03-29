namespace urban_dukan_checkout_service.Configurations
{
    public class RedisSettings
    {
        public string Configuration { get; set; } = "";
        public bool UseSsl { get; set; } = false;
        public int CartTtlHours { get; set; } = 24;
    }
}