using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using Shouldly;

namespace SprayChronicle.Testing
{
    public static class ObjectExtensions
    {
        public static object ShouldBeDeepEqualTo(this object actual, object expectation)
        {
            Render(expectation)
                .Equals(Render(actual))
                .ShouldBeTrue(Diff(actual, expectation));
            
            return actual;
        }
        
        private static string Diff(object left, object right)
        {
            var stringBuilder = new StringBuilder().AppendLine();
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(Render(left), Render(right));

            foreach (var line in diff.Lines) {
                switch (line.Type) {
                    case ChangeType.Inserted:
                        stringBuilder.Append("+ ");
                        break;
                    case ChangeType.Deleted:
                        stringBuilder.Append("- ");
                        break;
                    case ChangeType.Unchanged:
                        break;
                    case ChangeType.Imaginary:
                        break;
                    case ChangeType.Modified:
                        break;
                    default:
                        stringBuilder.Append("  ");
                        break;
                }

                stringBuilder.AppendLine(line.Text);
            }
            
            return stringBuilder.ToString();
        }

        private static string Render(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
