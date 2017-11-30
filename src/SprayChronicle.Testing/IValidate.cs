using System;

namespace SprayChronicle.Testing
{
    public interface IValidate<T>
    {
		IValidate<T> Expect();

		IValidate<T> Expect(int count);

		IValidate<T> Expect(params object[] results);

		IValidate<T> Expect(params Type[] types);

		IValidate<T> ExpectNoException();

		IValidate<T> ExpectException(Type type);

		IValidate<T> ExpectException(string message);
    }
}
