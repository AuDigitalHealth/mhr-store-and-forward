using DigitalHealth.StoreAndForward.Core.Queue.Models;
using FluentValidation;

namespace DigitalHealth.StoreAndForward.Core.Queue.Validators
{
    /// <summary>
    /// Queue document data validator.
    /// </summary>
    public class QueueDocumentDataValidator : AbstractValidator<QueueDocumentData>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public QueueDocumentDataValidator()
        {
            RuleFor(document => document.CdaPackage).NotEmpty();
            RuleFor(document => document.FormatCode).NotEmpty().MaximumLength(50);
            RuleFor(document => document.FormatCodeName).NotEmpty();
            RuleFor(document => document.DocumentIdToReplace).MaximumLength(50);
        }
    }
}