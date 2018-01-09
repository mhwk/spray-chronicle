using System;

namespace SprayChronicle.Testing
{
    public interface IValidate
    {
	    DateTime Epoch(int index);
	    
		IValidate Expect();

		IValidate Expect(int count);

		IValidate Expect(params object[] expectation);

		IValidate Expect(params Type[] expectation);

		IValidate ExpectNoException();

		IValidate ExpectException(Type type);

		IValidate ExpectException(string message);
    }
}
