using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Buratino.Xtensions
{
    public static class ReflectXtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod(int index = 1)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(index);
            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Возвращает свойства, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropsByAttribute<T>(this object obj) where T : Attribute
        {
            List<PropertyInfo> properties = new();
            foreach (var item in obj.GetType().GetProperties())
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                properties.Add(item);
            }
            return properties;
        }

        /// <summary>
        /// Возвращает методы, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsByAttribute<T>(this object obj) where T : Attribute
        {
            List<MethodInfo> methods = new();
            foreach (var item in obj.GetType().GetMethods())
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                methods.Add(item);
            }
            return methods;
        }

        /// <summary>
        /// Возвращает свойства, имеющие атрибут
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<MethodInfo, T>> GetMethodsWithAttribute<T>(this object obj) where T : Attribute
        {
            List<KeyValuePair<MethodInfo, T>> keyValuePairs = new();
            var all = obj.GetType().GetMethods();
            foreach (var item in obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                var valueAttributes = item.GetCustomAttributes<T>(false);
                if (valueAttributes is null)
                    continue;
                if (valueAttributes.Count() == 0)
                    continue;
                keyValuePairs.Add(new KeyValuePair<MethodInfo, T>(item, valueAttributes.First()));
            }
            return keyValuePairs;
        }

        /// <summary>
        /// Возвращает атрибут свойства
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            var valueAttributes = property.GetCustomAttributes<T>(false);
            if (valueAttributes is null)
                return null;
            if (valueAttributes.Count() == 0)
                return null;
            T description = valueAttributes.First();
            return description;
        }

        /// <summary>
        /// Возвращает атрибут перечисления
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attribute = memInfo[0].GetCustomAttributes(typeof(T), false).FirstOrDefault();
            return attribute != null ? (T)attribute : null;
        }

        public static string CD { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        /// <summary>
        /// Рендерит HTML
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="controller"></param>
        /// <param name="viewNamePath"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static async Task<string> RenderViewToStringAsync<TModel>(this Controller controller, string viewNamePath, TModel model)
        {
            if (string.IsNullOrEmpty(viewNamePath))
                viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                    ViewEngineResult viewResult = null;

                    if (viewNamePath.EndsWith(".cshtml"))
                        viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
                    else
                        viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, false);

                    if (!viewResult.Success)
                        return $"A view with the name '{viewNamePath}' could not be found";

                    ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        writer,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);

                    var res = writer.GetStringBuilder().ToString();
                    //byte[] unicodeBytes = Encoding.Unicode.GetBytes(res);
                    //string utf8 = Encoding.UTF8.GetString(unicodeBytes);
                    return res;
                }
                catch (Exception exc)
                {
                    return $"Failed - {exc.Message}";
                }
            }
        }

        /// <summary>
        /// Render a partial view to string, without a model present.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="controller"></param>
        /// <param name="viewNamePath"></param>
        /// <returns></returns>
        public static async Task<string> RenderViewToStringAsync(this Controller controller, string viewNamePath)
        {
            if (string.IsNullOrEmpty(viewNamePath))
                viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                    ViewEngineResult viewResult = null;

                    if (viewNamePath.EndsWith(".cshtml"))
                        viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
                    else
                        viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, false);

                    if (!viewResult.Success)
                        return $"A view with the name '{viewNamePath}' could not be found";

                    ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        writer,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);

                    return writer.GetStringBuilder().ToString();
                }
                catch (Exception exc)
                {
                    return $"Failed - {exc.Message}";
                }
            }
        }

        /// <summary>
        /// Вызывает метод у объекта
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <param name="genericTypes"></param>
        /// <returns></returns>
        public static object InvokeMethod(this object obj, string methodName, object[] args = null, Type[] genericTypes = null)
        {
            Type[] methodTypes = args == null ? new Type[0] : args.Select(x => x.GetType()).ToArray();
            int genericCount = genericTypes == null ? 0 : genericTypes.Length;
            MethodInfo method = null;
            if (obj is Type type)
            {
                var allM = type.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "AsQueryable" && x.IsGenericMethod);
                var param = allM.GetParameters();
                method = obj.GetType().GetMethod(methodName, genericCount, methodTypes);
            }
            else
            {
                type = null;
                if (genericCount > 0)
                {
                    method = obj.GetType().GetMethod(methodName);
                    method = method.MakeGenericMethod(genericTypes);
                }
                else
                {
                    method = obj.GetType().GetMethod(methodName);
                }
            }

            object result = null;
            if (type != null)
            {
                result = method.Invoke(null, args);
            }
            else
            {
                result = method.Invoke(obj, args);
            }
            return result;
        }

        /// <summary>
        /// Возвращает обобщенный метод
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="parameterTypes"></param>
        /// <returns></returns>
        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает значение свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetValProp(this object obj, string propName)
        {
            var prop = obj.GetProp(propName);
            object result;
            if (prop == null)
            {
                return null;
            }
            result = prop.GetValue(obj);
            return result;
        }

        /// <summary>
        /// Возвращает значение строкового свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string GetStringProp(this object obj, string propName)
        {
            return obj.GetValProp(propName).NullOrEmpty("").ToString();
        }

        /// <summary>
        /// Возвращает значение числового свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static int GetIntProp(this object obj, string propName)
        {
            return (int)obj.GetValProp(propName);
        }

        /// <summary>
        /// Возвращает значение числового свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static long GetLongProp(this object obj, string propName)
        {
            return (long)obj.GetValProp(propName);
        }

        /// <summary>
        /// Возвращает значение булева свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool GetBoolProp(this object obj, string propName)
        {
            return (bool)obj.GetValProp(propName);
        }

        /// <summary>
        /// Устанавливает значение свойства
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetProp(this object obj, string propName, object value)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return false;
            prop.SetValue(obj, value);
            return true;
        }

        /// <summary>
        /// Возвращает свойство
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProp(this object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return prop;
        }

        /// <summary>
        /// Проверяет, является ли объект массивом
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsArray(this object array)
        {
            if (array == null)
                throw new Exception("Нащальника, тут проблема. В метод IsArray попал NULL");
            return array is IEnumerable<object>;
        }

        /// <summary>
        /// Проверяет, является ли свойство массивом
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool PropIsArray(this PropertyInfo prop)
        {
            if (prop.PropertyType.Name == "String")
                return false;
            var res = prop.PropertyType.GetInterfaces().Any(x => x.Name == "IEnumerable");
            return res;
        }

        /// <summary>
        /// Преобразует объект в массив
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<object> AsArray(this object array)
        {
            if (array == null)
                return new object[0].Select(x => x);
            return array as IEnumerable<object>;
        }

        /// <summary>
        /// Динамичиский каст к нужному типу
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static dynamic CastDynamic(this object source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }

        /// <summary>
        /// Сортирует свойства: id, singleProps, arrayProps
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static PropertyInfo[] SortByImportantly(this PropertyInfo[] source)
        {
            var res = source.Where(x => x.Name == "Id")
                .Concat(source.Where(x => x.Name != "Id" && !x.PropIsArray()))
                .Concat(source.Where(x => x.Name != "Id" && x.PropIsArray()))
                .ToArray();
            return res;
        }

        /// <summary>
        /// Проверяет, является ли тип имплементацией базового типа
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsImplementationOfClass(this Type type, Type baseType)
        {
            if (type == null)
                return false;
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Any(x => x.FullName == baseType.FullName);
            }
            if (type.Name == baseType.Name)
                return true;
            return type.BaseType.IsImplementationOfClass(baseType);
        }

        /// <summary>
        /// Возвращает имплементации базового типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetImplementations(this Type type)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> types = assemblies.SelectMany(s => s.GetTypes()).OrderBy(x => x.Name).ToArray();

            var entityMappers = types
                .Where(p =>
                p.IsClass
                && !p.IsAbstract
                && p.IsImplementationOfClass(type))
                .OrderBy(x => x.Name)
                .ToArray();

            return entityMappers;
        }
    }
}