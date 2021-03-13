using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    [AttributeUsage(AttributeTargets.Method)]
    class EventSubscribe : Attribute
    {
        public double Priority;

        public EventSubscribe(double priority = 0)
        {
            Priority = priority;
        }
    }

    class Event
    {

    }

    class MethodTransformer
    {
        MethodInfo Method;
        Type Type;
        Type Action;
        double Priority;

        public MethodTransformer(MethodInfo method, Type type, double priority)
        {
            Method = method;
            Type = type;
            Action = GetAction(type);
            Priority = priority;
        }

        public EventHandler Transform()
        {
            Delegate del = Delegate.CreateDelegate(Action, Method);
            return new EventHandler(null, Type, del, Priority);
        }

        public EventHandler Transform(object o)
        {
            Delegate del = Delegate.CreateDelegate(Action, o, Method);
            return new EventHandler(o, Type, del, Priority);
        }

        private Type GetAction(Type type)
        {
            return typeof(Action<object>).GetGenericTypeDefinition().MakeGenericType(new[] { type });
        }
    }

    class ClassTransformer
    {
        List<MethodTransformer> Methods = new List<MethodTransformer>();
        List<MethodTransformer> MethodsStatic = new List<MethodTransformer>();

        public ClassTransformer(Type type)
        {
            var methods = type.GetRuntimeMethods();
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<EventSubscribe>();
                if (!method.IsPrivate && attribute != null)
                {
                    var parameters = method.GetParameters();
                    if (MatchesParameters(type, parameters))
                    {
                        Type parameterType = parameters[0].ParameterType;
                        if (method.IsStatic)
                            MethodsStatic.Add(new MethodTransformer(method, parameterType, attribute.Priority));
                        else
                            Methods.Add(new MethodTransformer(method, parameterType, attribute.Priority));
                    }
                }
            }
        }

        public IEnumerable<EventHandler> Transform()
        {
            foreach (var method in MethodsStatic)
            {
                yield return method.Transform();
            }
        }

        public IEnumerable<EventHandler> Transform(object o)
        {
            foreach (var method in Methods)
            {
                yield return method.Transform(o);
            }
        }

        private bool MatchesParameters(Type objectType, ParameterInfo[] parameters)
        {
            if (parameters.Length == 1)
                return typeof(Event).IsAssignableFrom(parameters[0].ParameterType);
            return false;
        }
    }

    class ComparerEventHandler : IComparer<EventHandler>
    {
        public int Compare(EventHandler x, EventHandler y)
        {
            return x.Priority.CompareTo(y.Priority);
        }
    }

    class EventHandler
    {
        public static IComparer<EventHandler> Comparer = new ComparerEventHandler();

        public object Object;
        Type Type;
        Delegate Delegate;
        public double Priority;

        public EventHandler(object obj, Type type, Delegate del, double priority)
        {
            Object = obj;
            Type = type;
            Delegate = del;
            Priority = priority;
        }

        public void Push(Event evt)
        {
            if (Type == evt.GetType())
                Delegate.DynamicInvoke(evt);
        }
    }

    class EventBus
    {
        static List<EventHandler> Handlers = new List<EventHandler>();

        public delegate void EventDelegate(Event evt);

        static Dictionary<Type, ClassTransformer> ClassTransformers = new Dictionary<Type, ClassTransformer>();

        public static void Register(Type type)
        {
            ClassTransformer classTransformer = ClassTransformers.GetOrDefault(type, null);
            if (classTransformer == null)
            {
                classTransformer = new ClassTransformer(type);
                ClassTransformers.Add(type, classTransformer);
            }

            Handlers.AddRange(classTransformer.Transform());
            Handlers.Sort(EventHandler.Comparer);
        }

        public static void Register(object o)
        {
            Type type = o.GetType();
            ClassTransformer classTransformer = ClassTransformers.GetOrDefault(type, null);
            if (classTransformer == null)
            {
                classTransformer = new ClassTransformer(type);
                ClassTransformers.Add(type, classTransformer);
            }

            Handlers.AddRange(classTransformer.Transform(o));
            Handlers.Sort(EventHandler.Comparer);
        }

        public static void Unregister(object o)
        {
            Handlers.RemoveAll(handler => handler.Object == o);
        }

        public static void PushEvent(Event evt)
        {
            foreach (var handler in Handlers.ToList())
            {
                handler.Push(evt);
            }
        }
    }
}
