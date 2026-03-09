using Domain.ValueObjects;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Guest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public Collection<Booking> Books { get; set; }
    public PersonId DocumentId { get; set; }
}
