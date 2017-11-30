using System;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;

namespace SprayChronicle.Testing
{
    public abstract class Validator<T> : IValidate<T>
    {
        private static readonly JsonDiffPatch _diff = new JsonDiffPatch();
        
        protected string Diff(object left, object right)
        {
//            var diff = _diff.Diff(
//                JsonConvert.SerializeObject(left, Formatting.Indented),
//                JsonConvert.SerializeObject(right, Formatting.Indented)
//            );
            var diff = string.Format(
                "{0}\nis not equal to\n{1}",
                JsonConvert.SerializeObject(left, Formatting.Indented),
                JsonConvert.SerializeObject(right, Formatting.Indented)
            );
            diff = diff?.Replace("{", "{{");
            diff = diff?.Replace("}", "}}");
            return diff;
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