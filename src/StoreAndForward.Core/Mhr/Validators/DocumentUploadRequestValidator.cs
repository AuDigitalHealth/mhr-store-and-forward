using DigitalHealth.StoreAndForward.Core.Mhr.Models;
using FluentValidation;

namespace DigitalHealth.StoreAndForward.Core.Mhr.Validators
{
    /// <summary>
    /// Document upload request validator.
    /// </summary>
    public class DocumentUploadRequestValidator : AbstractValidator<DocumentUploadRequest>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentUploadRequestValidator()
        {
            RuleFor(a => a.DocumentData).NotNull().NotEmpty();
            RuleFor(a => a.FormatCode).NotNull().NotEmpty();
            RuleFor(a => a.FormatCodeName).NotNull().NotEmpty();
        }
    }
}