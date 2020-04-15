namespace WoodGroveGroceriesWebApplication.Models.Settings
{
    using Microsoft.Extensions.Configuration;

    public abstract class ConfigOptionsBase<T>
        where T : ConfigOptionsBase<T>, new()
    {
        protected abstract string SectionName { get; }

        public static T Construct(IConfiguration configuration)
        {
            var instance = new T();
            return configuration.GetSection(instance.SectionName).Get<T>();
        }
    }
}