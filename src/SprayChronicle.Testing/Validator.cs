using System;
using System.Collections.Generic;
using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public abstract class Validator : IValidate
    {
        private static readonly JsonDiffPatch _diff = new JsonDiffPatch();
        
        protected string Diff(IEnumerable<Type> left, IEnumerable<Type> right)
        {
            return Escape(string.Format(
                "{0}\nis not equal to\n{1}",
                JsonConvert.SerializeObject(left.Select(l => l.FullName).ToArray(), Formatting.Indented),
                right.GetType().FullName,
                JsonConvert.SerializeObject(right.Select(r => r.FullName).ToArray(), Formatting.Indented)
            ));
        }
        
        protected string Diff(object left, object right)
        {
            return Escape(string.Format(
                "\n{0}\nis not equal to\n{1}\n",
                Render(left),
                Render(right)
            ));
        }

        private static string Render(object obj)
        {
            if (obj is IEnumerable<object> objs) {
                return string.Join(",\n", objs.Select(Render));
            }
            
            return string.Format(
                "{0} {1}",
                obj.GetType().FullName,
                JsonConvert.SerializeObject(obj, Formatting.Indented)
            );
        }

        private static string Escape(string str)
        {
            str = str?.Replace("{", "{{");
            str = str?.Replace("}", "}}");
            return str;
        }

        public abstract IValidate Expect();
        public abstract IValidate Expect(int count);
        public abstract IValidate Expect(params object[] results);
        public abstract IValidate Expect(params Type[] types);
        public abstract IValidate ExpectNoException();
        public abstract IValidate ExpectException(Type type);
        public abstract IValidate ExpectException(string message);
    }
}