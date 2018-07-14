using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSAT
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DomainAttribute : Attribute
    {
        public readonly string DomainName;

        public DomainAttribute(string domainName)
        {
            DomainName = domainName;
        }

        public DomainAttribute(string domainName, params string[] domainElements)
        {
            DomainName = domainName;
            if (!VariableType.TypeExists(domainName))
                // ReSharper disable once ObjectCreationAsStatement
                new FDomain<string>(domainName, domainElements);
        }

        public VariableType Domain => VariableType.TypeNamed(DomainName);
    }
}
