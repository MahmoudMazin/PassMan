using Microsoft.Extensions.Localization;
using System.Reflection;

namespace PassManNew.Resources
{
     public class LocalizationService
    {
        private readonly IStringLocalizer _localizer;

        public LocalizationService(IStringLocalizerFactory factory)
        {
            var type = typeof(Resource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create("Resource", assemblyName.Name);
        }

        public LocalizedString GetLocalized(string key)
        {
            return _localizer[key];
        }
    }

    public class Resource
    {
    }
}
