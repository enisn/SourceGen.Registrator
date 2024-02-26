using System;

namespace SourceGen.Registrator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class RegisterAsSingletonAttribute : Attribute
{
	public RegisterAsSingletonAttribute(params Type[] serviceTypes)
	{
	}
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class RegisterAsTransientAttribute : Attribute
{
    public RegisterAsTransientAttribute(params Type[] serviceTypes)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class RegisterAsScopedAttribute : Attribute
{
    public RegisterAsScopedAttribute(params Type[] serviceTypes)
    {
    }
}
