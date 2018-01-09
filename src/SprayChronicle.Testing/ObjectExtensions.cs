using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using Xunit;

namespace SprayChronicle.Testing
{
    public static class ObjectExtensions
    {
        public static object ShouldBeDeepEqualTo(this object actual, object expectation)
        {
            Assert.True(Render(expectation).Equals(Render(actual)), Diff(expectation, actual));
            
            return actual;
        }
        
        private static string Diff(object left, object right)
        {
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(Render(left), Render(right));
            var stringBuilder = new StringBuilder()
                .AppendLine()
                .AppendLine("- expected")
                .AppendLine("+ actual")
                .AppendLine();

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
