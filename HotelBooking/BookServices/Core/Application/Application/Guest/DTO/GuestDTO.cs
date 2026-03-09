using Domain.ValueObjects;

namespace Application.Guest.DTO;

public class GuestDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string IdNumber  { get; set; }
    public int IdTypeCode { get; set; }

    public static Domain.Entities.Guest MapToEntity(GuestDTO guestDTO)
    {
        return new Domain.Entities.Guest
        {
            Id = guestDTO.Id,
            Name = guestDTO.Name,
            Email = guestDTO.Email,
            DocumentId = new PersonId
            {
                IdNumber = guestDTO.IdNumber,
                Document = (DocumentType)guestDTO.IdTypeCode
            }
        };
    }
}
