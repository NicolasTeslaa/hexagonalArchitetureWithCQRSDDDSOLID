using Application.Book.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Book.Commands
{
    public class GetBookingQuery : IRequest<BookResponse>
    {
        public int Id { get; set; }
    }
}
