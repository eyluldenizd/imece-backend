using Core.Common.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public sealed class IdRequest
    {
        [Validate(
            ValidationRuleType.GreaterThan,
            0,
            ErrorMessage = "ID değeri sıfırdan büyük olmalıdır.")]
        public long Id { get; set; }
    }
}
