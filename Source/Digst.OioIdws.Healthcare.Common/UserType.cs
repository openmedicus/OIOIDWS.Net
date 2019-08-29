﻿

using Digst.OioIdws.SamlAttributes;

namespace Digst.OioIdws.Healthcare.Common
{
    public enum UserType
    {
        [SamlAttributeValue("Citizen")]
        Citizen = 1,

        [SamlAttributeValue("Employee")]
        Employee = 2,

        [SamlAttributeValue("System")]
        System = 3
    }
}