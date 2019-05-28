using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;

namespace foundation.configuration
{
    public class Config : ConfigurationSection, IConfigurationSectionHandler
    {
        public string Note { get; private set; }
        public object Create(object parent, object configContext, XmlNode section)
        {
            var config = new Config();
            var xe = section.SelectSingleNode("Note");
            if (xe != null && xe.Attributes != null)
            {
                var attr = xe.Attributes["value"];
                if (attr != null)
                    config.Note = attr.Value;
            }
            return config;
        }

        [ConfigurationProperty("nodes", IsRequired = true)]
        public ConfigElementCollection Nodes
        {
            get { return (ConfigElementCollection)base["nodes"]; }
        }
    }

    public class ConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = false, IsKey = false), ConfigurationValidator(typeof(NameValidator))]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
        [ConfigurationProperty("key", IsRequired = true, IsKey = true), IntegerValidator(MaxValue = 100, MinValue = 1)]
        public int Key
        {
            get { return (int)base["key"]; }
            set { base["key"] = value; }
        }
        private class NameValidator : ConfigurationValidatorBase
        {
            public override bool CanValidate(Type type)
            {
                return (type == typeof(string) || base.CanValidate(type));
            }

            public override void Validate(object value)
            {
                string name = value as string;
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("A string is need here.");
                if (!name.EndsWith("Ex"))
                    throw new ArgumentException("Value is invalid. Please make sure it ends with \"Ex\"");
            }
        }
    }

    public class ConfigElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            ConfigElement e = element as ConfigElement;
            if (e != null)
            {
                return string.Concat(e.Name, ":", e.Key);
            }
            return string.Empty;
        }

        public IDictionary<int, string> ToDictionary()
        {
            IDictionary<int, string> dict = new Dictionary<int, string>();
            foreach (ConfigElement e in this)
            {
                dict.Add(e.Key, e.Name);
            }
            return dict;
        }
    }
}
