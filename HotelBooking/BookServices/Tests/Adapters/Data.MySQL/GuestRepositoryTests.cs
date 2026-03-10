using Data.MySql;
using Data.MySql.Repositories;
using Domain.Guest.Entities;
using Domain.Guest.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Data.MySQL;

public class GuestRepositoryTests
{
    private static HotelDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HotelDbContext(options);
    }

    private static Guest CreateGuest(
        int id = 0,
        string name = "Tesla Silva",
        string email = "tesla@gmail.com",
        string idNumber = "12345678901",
        int documentType = 1)
    {
        return new Guest
        {
            Id = id,
            Name = name,
            Email = email,
            DocumentId = new PersonId
            {
                IdNumber = idNumber,
                Document = (DocumentType)documentType
            }
        };
    }

    [Fact]
    public async Task AddGuestAsync_Should_Add_Guest_And_Return_Id()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest();

        var id = await repository.AddGuestAsync(guest);

        Assert.True(id > 0);

        var savedGuest = await context.Guest.FirstOrDefaultAsync(g => g.Id == id);
        Assert.NotNull(savedGuest);
        Assert.Equal("Tesla Silva", savedGuest.Name);
        Assert.Equal("tesla@gmail.com", savedGuest.Email);
    }

    [Fact]
    public async Task DeleteGuestAsync_Should_Return_True_When_Guest_Exists()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest();

        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var result = await repository.DeleteGuestAsync(guest.Id);

        Assert.True(result);
        Assert.Empty(context.Guest);
    }

    [Fact]
    public async Task DeleteGuestAsync_Should_Return_False_When_Guest_Does_Not_Exist()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);

        var result = await repository.DeleteGuestAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task FindByEmail_Should_Return_Guest_When_Email_Exists()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest(email: "findme@gmail.com");

        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var result = await repository.FindByEmail("findme@gmail.com");

        Assert.NotNull(result);
        Assert.Equal("findme@gmail.com", result.Email);
    }

    [Fact]
    public async Task FindByEmail_Should_Return_Null_When_Email_Does_Not_Exist()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);

        var result = await repository.FindByEmail("naoexiste@gmail.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task FindById_Should_Return_Guest_When_Id_Exists()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest();

        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var result = await repository.FindById(guest.Id);

        Assert.NotNull(result);
        Assert.Equal(guest.Id, result.Id);
        Assert.Equal(guest.Name, result.Name);
    }

    [Fact]
    public async Task FindById_Should_Return_Null_When_Id_Does_Not_Exist()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);

        var result = await repository.FindById(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllGuestsAsync_Should_Return_All_Guests()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);

        context.Guest.AddRange(
            CreateGuest(name: "Guest 1", email: "guest1@gmail.com"),
            CreateGuest(name: "Guest 2", email: "guest2@gmail.com")
        );
        await context.SaveChangesAsync();

        var result = await repository.GetAllGuestsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGuestByIdAsync_Should_Return_Guest_When_Id_Exists()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest();

        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        var result = await repository.GetGuestByIdAsync(guest.Id);

        Assert.NotNull(result);
        Assert.Equal(guest.Id, result.Id);
        Assert.Equal(guest.Email, result.Email);
    }

    [Fact]
    public async Task GetGuestByIdAsync_Should_Return_Empty_Guest_When_Id_Does_Not_Exist()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);

        var result = await repository.GetGuestByIdAsync(999);

        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Null(result.Name);
        Assert.Null(result.Email);
    }

    [Fact]
    public async Task UpdateGuestAsync_Should_Update_Guest_And_Return_True()
    {
        using var context = CreateContext();
        var repository = new GuestRepository(context);
        var guest = CreateGuest();

        context.Guest.Add(guest);
        await context.SaveChangesAsync();

        guest.Name = "Tesla Atualizado";
        guest.Email = "tesla.atualizado@gmail.com";

        var result = await repository.UpdateGuestAsync(guest);

        Assert.True(result);

        var updatedGuest = await context.Guest.FirstOrDefaultAsync(g => g.Id == guest.Id);
        Assert.NotNull(updatedGuest);
        Assert.Equal("Tesla Atualizado", updatedGuest.Name);
        Assert.Equal("tesla.atualizado@gmail.com", updatedGuest.Email);
    }
}
