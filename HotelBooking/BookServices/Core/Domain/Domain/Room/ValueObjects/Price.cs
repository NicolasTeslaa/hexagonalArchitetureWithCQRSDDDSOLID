using Domain.Room.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Room.ValueObjects;

public class Price
{
    public decimal Value {  get; set; }
    public AcceptedCurrencies Currency { get; set; }
}
