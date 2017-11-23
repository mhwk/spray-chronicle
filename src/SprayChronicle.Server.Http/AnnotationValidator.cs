using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace SprayChronicle.Server.Http
{
    public sealed class AnnotationValidator : IValidator
    {
        public AnnotationValidator()
        {
        }

        public void Validate(object payload)
        {
            try {
                Validator.ValidateObject(payload, new ValidationContext(payload), true);
            } catch (ValidationException error) {
                throw new InvalidRequestException(string.Format("Invalid message: {0}", error.Message), error);
            }
        }
    }
}
