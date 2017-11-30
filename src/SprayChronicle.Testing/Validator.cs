using System;
using System.Collections.Generic;
using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public abstract class Validator<T> : IValidate<T>
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
                "{0} {1}\nis not equal to\n{2} {3}",
                left.GetType().FullName,
                JsonConvert.SerializeObject(left, Formatting.Indented),
                right.GetType().FullName,
                JsonConvert.SerializeObject(right, Formatting.Indented)
            ));
        }
        
        private static string Escape(string str)
        {
            str = str?.Replace("{", "{{");
            str = str?.Replace("}", "}}");
            return str;
        }

        public abstract IValidate<T> Expect();
        public abstract IValidate<T> Expect(int count);
        public abstract IValidate<T> Expect(params object[] results);
        public abstract IValidate<T> Expect(params Type[] types);
        public abstract IValidate<T> ExpectNoException();
        public abstract IValidate<T> ExpectException(Type type);
        public abstract IValidate<T> ExpectException(string message);
    }
}