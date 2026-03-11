using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payment.DTO;

public enum PaymentState
{
    Success = 0,
    Failed = 1,
    Error = 2,
    Undefined = 3
}

public class PaymentStateDTO
{
    public PaymentState State { get; set; }
    public int PaymentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; }
}
