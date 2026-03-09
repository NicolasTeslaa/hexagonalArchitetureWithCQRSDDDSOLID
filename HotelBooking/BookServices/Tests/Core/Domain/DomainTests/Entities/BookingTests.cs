using Domain.Entities;
using Domain.Enums;
using System.Reflection;
using Action = Domain.Enums.Action;

namespace DomainTests.Entities;

public class BookingTests
{
    [Theory]
    [InlineData(Status.Created, Action.Pay, Status.Paid)]
    [InlineData(Status.Created, Action.Cancel, Status.Canceled)]
    [InlineData(Status.Paid, Action.Finish, Status.Finished)]
    [InlineData(Status.Paid, Action.Refound, Status.Refunded)]
    [InlineData(Status.Canceled, Action.ReOpen, Status.Created)]
    public void ChangeState_Deve_Alterar_Status_Quando_Transicao_For_Valida(
        Status statusInicial,
        Action acao,
        Status statusEsperado)
    {
        // Arrange
        var booking = CriarBookingComStatus(statusInicial);

        // Act
        booking.ChangeState(acao);

        // Assert
        Assert.Equal(statusEsperado, booking.CurrentStatus);
    }

    [Theory]
    [InlineData(Status.Created, Action.Finish)]
    [InlineData(Status.Created, Action.Refound)]
    [InlineData(Status.Created, Action.ReOpen)]

    [InlineData(Status.Paid, Action.Pay)]
    [InlineData(Status.Paid, Action.Cancel)]
    [InlineData(Status.Paid, Action.ReOpen)]

    [InlineData(Status.Canceled, Action.Cancel)]
    [InlineData(Status.Canceled, Action.Finish)]
    [InlineData(Status.Canceled, Action.Refound)]
    [InlineData(Status.Canceled, Action.Pay)]

    [InlineData(Status.Finished, Action.Pay)]
    [InlineData(Status.Finished, Action.Cancel)]
    [InlineData(Status.Finished, Action.Finish)]
    [InlineData(Status.Finished, Action.Refound)]
    [InlineData(Status.Finished, Action.ReOpen)]

    [InlineData(Status.Refunded, Action.Pay)]
    [InlineData(Status.Refunded, Action.Cancel)]
    [InlineData(Status.Refunded, Action.Finish)]
    [InlineData(Status.Refunded, Action.Refound)]
    [InlineData(Status.Refunded, Action.ReOpen)]
    public void ChangeState_Nao_Deve_Alterar_Status_Quando_Transicao_For_Invalida(
        Status statusInicial,
        Action acao)
    {
        // Arrange
        var booking = CriarBookingComStatus(statusInicial);

        // Act
        booking.ChangeState(acao);

        // Assert
        Assert.Equal(statusInicial, booking.CurrentStatus);
    }

    [Fact]
    public void CurrentStatus_Deve_Retornar_Status_Atual()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Paid);

        // Act
        var statusAtual = booking.CurrentStatus;

        // Assert
        Assert.Equal(Status.Paid, statusAtual);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Paid_Para_Finished()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Finish);

        // Assert
        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Canceled_Para_ReOpen()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.ReOpen);

        // Assert
        Assert.Equal(Status.Created, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Paid_Para_Refunded()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Refound);

        // Assert
        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Apos_Finished_Nao_Deve_Alterar_Mais_O_Status()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Paid);
        booking.ChangeState(Action.Finish);

        // Act
        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.Refound);
        booking.ChangeState(Action.ReOpen);

        // Assert
        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Apos_Refunded_Nao_Deve_Alterar_Mais_O_Status()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Paid);
        booking.ChangeState(Action.Refound);

        // Act
        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.Finish);
        booking.ChangeState(Action.ReOpen);

        // Assert
        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Quando_Cancelado_E_Reaberto_Deveria_Permitir_Pagamento_Novamente()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.ReOpen);
        booking.ChangeState(Action.Pay);

        // Assert
        Assert.Equal(Status.Paid, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Quando_Acao_Invalida_For_Chamada_Mais_De_Uma_Vez_Deve_Manter_O_Status()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Finish);
        booking.ChangeState(Action.Finish);
        booking.ChangeState(Action.Refound);
        booking.ChangeState(Action.ReOpen);

        // Assert
        Assert.Equal(Status.Created, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Sequencia_Mista_De_Acoes_Invalidas_E_Validas_Deve_Respeitar_A_Regra()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Finish);  // inválida
        booking.ChangeState(Action.Pay);     // válida -> Paid
        booking.ChangeState(Action.ReOpen);  // inválida
        booking.ChangeState(Action.Finish);  // válida -> Finished
        booking.ChangeState(Action.Cancel);  // inválida

        // Assert
        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Manter_Consistencia_Em_Varias_Transicoes()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);

        // Act
        booking.ChangeState(Action.Cancel);   // Canceled
        booking.ChangeState(Action.ReOpen);   // Created
        booking.ChangeState(Action.Pay);      // Paid
        booking.ChangeState(Action.Refound);  // Refunded
        booking.ChangeState(Action.ReOpen);   // inválida
        booking.ChangeState(Action.Pay);      // inválida

        // Assert
        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Nao_Deve_Afetar_Outras_Propriedades_Do_Objeto()
    {
        // Arrange
        var booking = CriarBookingComStatus(Status.Created);
        booking.Id = 10;
        booking.PlacedAt = new DateTime(2026, 03, 09, 10, 00, 00);
        booking.Start = new DateTime(2026, 03, 10, 14, 00, 00);
        booking.End = new DateTime(2026, 03, 10, 16, 00, 00);

        // Act
        booking.ChangeState(Action.Pay);

        // Assert
        Assert.Equal(10, booking.Id);
        Assert.Equal(new DateTime(2026, 03, 09, 10, 00, 00), booking.PlacedAt);
        Assert.Equal(new DateTime(2026, 03, 10, 14, 00, 00), booking.Start);
        Assert.Equal(new DateTime(2026, 03, 10, 16, 00, 00), booking.End);
        Assert.Equal(Status.Paid, booking.CurrentStatus);
    }

    private static Booking CriarBookingComStatus(Status status)
    {
        var booking = new Booking();

        var propriedadeStatus = typeof(Booking).GetProperty(
            "Status",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (propriedadeStatus is null)
            throw new InvalidOperationException("A propriedade privada 'Status' não foi encontrada.");

        propriedadeStatus.SetValue(booking, status);

        return booking;
    }
}