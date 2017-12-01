using System;

namespace SprayChronicle.Testing
{
    public interface IValidate
    {
		IValidate Expect();

		IValidate Expect(int count);

		IValidate Expect(params object[] results);

		IValidate Expect(params Type[] types);

		IValidate ExpectNoException();

		IValidate ExpectException(Type type);

		IValidate ExpectException(string message);
    }
}
