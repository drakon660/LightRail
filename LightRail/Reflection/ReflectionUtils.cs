using System.Collections.ObjectModel;
using System.Reflection;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Sigil.NonGeneric;

namespace LightRail.Reflection
{
    public static class ReflectionUtils
    {
        private static readonly IReadOnlyList<string> methodsToFilter = new[]
        {
            nameof(object.GetType),
            nameof(object.Equals),
            nameof(object.GetHashCode),
            nameof(object.ToString),
        };

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <returns></returns>
        // public static T GetCustomAttribute<T>(MemberInfo memberInfo)
        //     where T : class
        // {
        //     return GetCustomAttribute<T>(memberInfo, false);
        // }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <returns></returns>
        // public static T GetCustomAttribute<T>(ParameterInfo parameterInfo)
        //     where T : class
        // {
        //     return GetCustomAttribute<T>(parameterInfo, false);
        // }
        public static Dictionary<K, V> Merge<K, V>(IEnumerable<Dictionary<K, V>> dictionaries)
        {
            return dictionaries.SelectMany(x => x)
                .ToDictionary(x => x.Key, y => y.Value);
        }

        public static void GetCustomAttribute<T>(in PropertyInfo propertyInfo, ref IDictionary<string, T> map)
            where T : Attribute
        {
            var attribute = propertyInfo.GetCustomAttribute<T>();
            if (attribute is not null)
                map.Add($"{propertyInfo.DeclaringType}_{propertyInfo.Name}", attribute);

            if (IsSimpleType(propertyInfo.PropertyType)) return;
            foreach (var internalPropertyInfo in propertyInfo.PropertyType.GetProperties())
            {
                GetCustomAttribute<T>(internalPropertyInfo, ref map);
            }
        }

        public static IReadOnlyDictionary<string, T> GetCustomAttributes<T>(in Type type) where T : Attribute
        {
            IDictionary<string, T> customAttributes = new Dictionary<string, T>();

            foreach (var propertyInfo in type.GetProperties())
            {
                GetCustomAttribute(propertyInfo, ref customAttributes);
            }

            return customAttributes.AsReadOnly();
        }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(ParameterInfo parameterInfo, bool inherit)
            where T : class
        {
            object[] attributes = parameterInfo.GetCustomAttributes(typeof(T), inherit);
            if (attributes.Length == 0)
            {
                return null;
            }

            return (T)attributes[0];
        }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(MemberInfo memberInfo, bool inherit)
            where T : class
        {
            object[] attributes = memberInfo.GetCustomAttributes(typeof(T), inherit);
            if (attributes.Length == 0)
            {
                return null;
            }

            return (T)attributes[0];
        }

        /// <summary>
        /// Determines whether [has custom attribute] [the specified member info].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>
        ///   <c>true</c> if [has custom attribute] [the specified member info]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasCustomAttribute<T>(MemberInfo memberInfo, bool inherit)
            where T : class
        {
            return GetCustomAttribute<T>(memberInfo, inherit) != null;
        }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static List<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : class
        {
            object[] attributes = memberInfo.GetCustomAttributes(typeof(T), inherit);
            List<T> result = new List<T>(attributes.Length);
            for (int i = 0; i < attributes.Length; i++)
            {
                result.Add((T)attributes[i]);
            }

            return result;
        }

        /// <summary>
        /// Gets the name of the method by.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodByName(Type type, string methodName, IDictionary<string, Type> parameters)
        {
            return GetMethodByName(type, BindingFlags.Instance | BindingFlags.Public, methodName, parameters);
        }

        /// <summary>
        /// Gets the name of the method by.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodByName(Type type, BindingFlags bindingFlags, string methodName,
            IDictionary<string, Type> parameters)
        {
            MethodInfo[] methodInfos = type.GetMethods(bindingFlags);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                if (methodInfo.Name == methodName)
                {
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                    if (parameters == null)
                        return methodInfo;

                    if (parameterInfos.Length != parameters.Count)
                    {
                        continue;
                    }

                    bool parameterFound = true;
                    foreach (KeyValuePair<string, Type> parameter in parameters)
                    {
                        parameterFound = false;

                        for (int j = 0; j < parameterInfos.Length; j++)
                        {
                            ParameterInfo parameterInfo = parameterInfos[j];
                            if (parameterInfo.Name == parameter.Key && parameterInfo.ParameterType == parameter.Value)
                            {
                                parameterFound = true;
                                break;
                            }
                        }

                        if (!parameterFound)
                        {
                            break;
                        }
                    }

                    if (!parameterFound)
                    {
                        continue;
                    }

                    return methodInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="object">The @object.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object InvokeMethod(object @object, string methodName,
            IReadOnlyDictionary<string, MethodInputParameter> parameters)
        {
            return InvokeMethod(@object, methodName, BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="object">The @object.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object InvokeMethod(object @object, string methodName, BindingFlags bindingFlags,
            IReadOnlyDictionary<string, MethodInputParameter> parameters)
        {
            if (@object == null)
            {
                throw new ArgumentNullException("object");
            }

            Type objectType = @object.GetType();
            MethodInfo methodInfo = GetMethodByName(objectType, methodName,
                parameters?.ToDictionary(x => x.Key, y => y.Value.Type));

            if (methodInfo == null)
            {
                methodInfo = GetMethodByName(objectType, methodName, new Dictionary<string, Type>());
                if (methodInfo == null)
                    throw new ReflectionUtilsException(String.Format("'{0}' method of '{1}' type couldn't be found",
                        methodName, objectType.AssemblyQualifiedName));
            }

            return InvokeMethod(methodInfo, @object, BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="object">The @object.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object InvokeMethod(MethodInfo methodInfo, object @object,
            IReadOnlyDictionary<string, MethodInputParameter> parameters)
        {
            return InvokeMethod(methodInfo, @object, BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="object">The @object.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object InvokeMethod(MethodInfo methodInfo, object @object, BindingFlags bindingFlags,
            IReadOnlyDictionary<string, MethodInputParameter> parameters)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (@object == null)
            {
                throw new ArgumentNullException("object");
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            if (parameters == null)
            {
                return methodInfo.Invoke(@object, bindingFlags, null, new object[parameterInfos.Length],
                    CultureInfo.CurrentCulture);
                //throw new ArgumentNullException("parameters");
            }

            if (parameterInfos.Length != parameters.Count)
            {
                throw new ReflectionUtilsException(String.Format(
                    "'{0}' method parameters count doesn't match the specified parameters count", methodInfo));
            }

            object[] arguments = new object[parameters.Count];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo parameterInfo = parameterInfos[i];
                MethodInputParameter methodInputParameter;

                if (!parameters.TryGetValue(parameterInfo.Name, out methodInputParameter))
                {
                    throw new ReflectionUtilsException(String.Format("'{0}' parameter doesn't exist in '{1}' method",
                        parameterInfo.Name, methodInfo));
                }

                arguments[parameterInfo.Position] =
                    ConversionUtils.ChangeType(methodInputParameter.Value, methodInputParameter.Type);
            }

            return methodInfo.Invoke(@object, bindingFlags, null, arguments, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the method definition.
        /// </summary>
        /// <param name="object">The @object.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public static Method GetMethodDefinition(object @object, string methodName)
        {
            MethodInfo methodInfo = @object.GetType().GetMethod(methodName);
            return GetMethodDefinition(methodInfo);
        }

        public static Method GetMethodDefinition(Type type, string methodName)
        {
            MethodInfo methodInfo = type.GetMethod(methodName);
            return GetMethodDefinition(methodInfo);
        }

        public static Method GetMethodDefinition(MethodInfo methodInfo)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            Method method = new Method { Name = methodInfo.Name };
            Dictionary<Type, Class> classDefinitions = new Dictionary<Type, Class>();

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo parameterInfo = parameterInfos[i];
                Type parameterType = parameterInfo.ParameterType;
                Reflection.Parameter parameter = new Reflection.Parameter
                    { Name = parameterInfo.Name, Type = parameterType, Position = parameterInfo.Position };

                if (!parameterType.IsValueType && parameterType != typeof(string))
                {
                    Class classDefinition = GetClassDefinition(parameterType, classDefinitions);
                    parameter.Definition = classDefinition;
                    classDefinitions[parameterType] = classDefinition;
                }

                method.Parameters.Add(parameter);
            }

            Type returnType = methodInfo.ReturnType;
            if (returnType == typeof(Task))
            {
                return method;
            }


            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var genericArguments = returnType.GetGenericArguments();

                if (genericArguments.Length > 1)
                    throw new NotImplementedException("more than 1 generic parameters in result are not supported");

                var genericResultType = genericArguments[0]; //TODO is it faster then first?

                if (genericResultType.IsClass)
                {
                    Class returnClass = GetClassDefinition(genericResultType, classDefinitions);
                    method.ReturnValue = new Instance { Type = genericResultType, Definition = returnClass };
                }
            }
            else
            {
                Class returnClass = GetClassDefinition(returnType, classDefinitions);
                method.ReturnValue = new Instance { Type = returnType, Definition = returnClass };
            }

            return method;
        }

        public static IReadOnlyList<Method> GetMethodsDefinition(object @object)
        {
            //TODO skip method from base class
            MethodInfo[] methodsInfo =
                @object.GetType().GetMethods().Where(x => !methodsToFilter.Contains(x.Name)).ToArray();
            List<Method> methods = new List<Method>(methodsInfo.Length);

            for (int i = 0; i < methodsInfo.Length; i++)
            {
                methods.Add(GetMethodDefinition(methodsInfo[i]));
            }

            return methods.AsReadOnly();
        }

        public static IReadOnlyList<Method> GetMethodsDefinition(Type type)
        {
            //TODO skip method from base class
            MethodInfo[] methodsInfo = type.GetMethods().Where(x => !methodsToFilter.Contains(x.Name)).ToArray();
            List<Method> methods = new List<Method>(methodsInfo.Length);

            for (int i = 0; i < methodsInfo.Length; i++)
            {
                //var map = type.GetInterfaceMap(methodsInfo[i].DeclaringType);
                methods.Add(GetMethodDefinition(methodsInfo[i]));
            }

            return methods.AsReadOnly();
        }

        public static IReadOnlyList<Method> GetMethodsDefinition(Type type, string interfaceName)
        {
            var @interface = type.GetInterface(interfaceName);

            InterfaceMapping map = type.GetInterfaceMap(@interface);

            //TODO skip method from base class
            MethodInfo[] methodsInfo = map.InterfaceMethods;

            List<Method> methods = new List<Method>(methodsInfo.Length);

            for (int i = 0; i < methodsInfo.Length; i++)
            {
                methods.Add(GetMethodDefinition(methodsInfo[i]));
            }

            return methods.AsReadOnly();
        }

        /// <summary>
        /// Gets the class definition.
        /// </summary>
        /// <param name="classType">Type of the class.</param>
        /// <returns></returns>
        public static Class GetClassDefinition(Type classType)
        {
            return GetClassDefinition(classType, new Dictionary<Type, Class>());
        }

        /// <summary>
        /// Gets the class definition.
        /// </summary>
        /// <param name="classType">Type of the class.</param>
        /// <param name="classDefinitions">The class definitions.</param>
        /// <returns></returns>
        private static Class GetClassDefinition(Type classType, IDictionary<Type, Class> classDefinitions)
        {
            Class @class;
            if (classDefinitions.TryGetValue(classType, out @class))
            {
                return @class;
            }

            @class = new Class { Type = classType };
            if (classType.IsValueType || classType == typeof(string))
            {
                return @class;
            }

            PropertyInfo[] propertyInfos = classType.GetProperties();
            for (int j = 0; j < propertyInfos.Length; j++)
            {
                PropertyInfo propertyInfo = propertyInfos[j];
                Type propertyType = propertyInfo.PropertyType;
                Property property = new Property { Name = propertyInfo.Name, Type = propertyType };
                if (!propertyType.IsValueType && propertyType != typeof(string))
                {
                    if (classType == propertyType)
                    {
                        property.Definition = @class;
                    }
                    else
                    {
                        property.Definition = GetClassDefinition(propertyType, classDefinitions);
                    }
                }

                @class.Properties.Add(property);
            }

            classDefinitions[classType] = @class;
            @class.Definition = @class;
            return @class;
        }

        public struct MethodInputParameter : IEquatable<MethodInputParameter>
        {
            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public Type Type { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public object Value { get; set; }

            /// <summary>
            /// Implements the operator ==.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator ==(MethodInputParameter left, MethodInputParameter right)
            {
                return left.Type == right.Type && left.Value == right.Value;
            }

            /// <summary>
            /// Implements the operator !=.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>
            /// The result of the operator.
            /// </returns>
            public static bool operator !=(MethodInputParameter left, MethodInputParameter right)
            {
                return !(left == right);
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(MethodInputParameter other)
            {
                return Equals(other.Type, Type) && Equals(other.Value, Value);
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <returns>
            /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
            /// </returns>
            /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is MethodInputParameter && Equals((MethodInputParameter)obj);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                }
            }
        }

        public static Type CreateType(string typeName, IReadOnlyDictionary<string, Type> props)
        {
            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dtos"), AssemblyBuilderAccess.Run);
            var mod = asm.DefineDynamicModule("Dto");

            var typeBuilder =
                mod.DefineType(
                    typeName,
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                    typeof(object), null
                );


            foreach (var prop in props)
            {
                var propBuilder =
                    typeBuilder.DefineProperty(prop.Key, PropertyAttributes.None, prop.Value, Type.EmptyTypes);

                Func<FieldInfo> getBackingField;
                {
                    FieldInfo backingField = null;
                    getBackingField =
                        () =>
                        {
                            if (backingField == null)
                            {
                                backingField = typeBuilder.DefineField("_" + prop.Key + "_" + Guid.NewGuid(),
                                    prop.Value, FieldAttributes.Private);
                            }

                            return backingField;
                        };
                }

                var getterMethod = Emit.BuildInstanceMethod(prop.Value, Type.EmptyTypes, typeBuilder, prop.Key,
                    MethodAttributes.Public);

                getterMethod.LoadArgument(0);
                getterMethod.LoadField(getBackingField());
                getterMethod.Return();

                var getter = getterMethod.CreateMethod();
                propBuilder.SetGetMethod(getter);

                var setterMethod = Emit.BuildInstanceMethod(typeof(void), new[] { prop.Value },
                    typeBuilder, "set_" + prop.Key, MethodAttributes.Public);

                setterMethod.LoadArgument(0);
                setterMethod.LoadArgument(1);
                setterMethod.StoreField(getBackingField());
                setterMethod.Return();

                var setter = setterMethod.CreateMethod();

                propBuilder.SetSetMethod(setter);
            }

            var destinationType = typeBuilder.CreateTypeInfo();

            return destinationType;
        }

        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static object CreateInstance(ConstructorInfo constructorInfo)
        {
            return constructorInfo.Invoke(new object[0]);
        }

        // public static Func<object> CreateActivator(Type type)
        // {
        //     return Expression.Lambda<Func<object>>(Expression.New(type)).CompileFast();
        // }
        //
        // public static Action<object, object> CreateSetter(Type type, string name)
        // {
        //     PropertyInfo propertyInfo = type.GetProperty(name);
        //     ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
        //     UnaryExpression instanceExpresion = Expression.Convert(instance, propertyInfo.DeclaringType);
        //
        //     ParameterExpression propertyValue = Expression.Parameter(typeof(object), "propertyValue");
        //     UnaryExpression propertyValueExpression = Expression.Convert(propertyValue, propertyInfo.PropertyType);
        //
        //     var body = Expression.Assign(Expression.Property(instanceExpresion, propertyInfo.Name), propertyValueExpression);
        //     return Expression.Lambda<Action<object, object>>(body, instance, propertyValue).CompileFast();
        // }

        public static object GetValue(object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName).GetValue(instance);
        }

        public static Type FromString(string type)
        {
            return type switch
            {
                "string" => typeof(string),
                "Guid" => typeof(Guid),
                "int" => typeof(int),
                "decimal" => typeof(decimal),
                "double" => typeof(double),
                "float" => typeof(float),
                "byte" => typeof(byte),
                "short" => typeof(short),
                "long" => typeof(long),
                "bool" => typeof(bool),
                "datetime" => typeof(DateTime),
                "dateOnly" => typeof(DateOnly),
                _ => throw new NotImplementedException("No type map in FromString method")
            };
        }

        public static bool IsSimpleType(Type type)
        {
            return type switch
            {
                Type t when t == typeof(string) => true,
                Type t when t == typeof(bool) => true,
                Type t when t == typeof(char) => true,
                Type t when t == typeof(byte) => true,
                Type t when t == typeof(sbyte) => true,
                Type t when t == typeof(short) => true,
                Type t when t == typeof(ushort) => true,
                Type t when t == typeof(int) => true,
                Type t when t == typeof(uint) => true,
                Type t when t == typeof(long) => true,
                Type t when t == typeof(ulong) => true,
                Type t when t == typeof(float) => true,
                Type t when t == typeof(double) => true,
                Type t when t == typeof(decimal) => true,
                Type t when t == typeof(DateTime) => true,
                Type t when t == typeof(DateOnly) => true,
                Type t when t == typeof(TimeSpan) => true,
                Type t when t == typeof(Guid) => true,
                _ => false
            };
        }

        public static Type Merge(string name, IEnumerable<Reflection.Parameter> types)
        {
            return CreateType(name, types.ToDictionary(x => x.Name, y => y.Type));
        }
    }
}