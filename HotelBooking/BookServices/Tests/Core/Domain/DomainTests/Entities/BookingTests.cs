using Domain.Book.Entities;
using Domain.Book.Enums;
using Domain.Book.Exceptions;
using Action = Domain.Book.Enums.Action;

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
        var booking = CriarBookingComStatus(statusInicial);

        booking.ChangeState(acao);

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
    public void ChangeState_Deve_Lancar_Excecao_Quando_Transicao_For_Invalida(
        Status statusInicial,
        Action acao)
    {
        var booking = CriarBookingComStatus(statusInicial);

        var exception = Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(acao));

        Assert.NotNull(exception);
        Assert.Equal(statusInicial, booking.CurrentStatus);
    }

    [Fact]
    public void CurrentStatus_Deve_Retornar_Status_Atual()
    {
        var booking = CriarBookingComStatus(Status.Paid);

        var statusAtual = booking.CurrentStatus;

        Assert.Equal(Status.Paid, statusAtual);
    }

    [Fact]
    public void SetStatus_Deve_Alterar_Status_Manual()
    {
        var booking = new Booking();

        booking.SetStatus(Status.Canceled);

        Assert.Equal(Status.Canceled, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Paid_Para_Finished()
    {
        var booking = CriarBookingComStatus(Status.Created);

        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Finish);

        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Canceled_Para_ReOpen()
    {
        var booking = CriarBookingComStatus(Status.Created);

        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.ReOpen);

        Assert.Equal(Status.Created, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Permitir_Fluxo_Created_Para_Paid_Para_Refunded()
    {
        var booking = CriarBookingComStatus(Status.Created);

        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Refound);

        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Apos_Finished_Deve_Lancar_Excecao_Para_Qualquer_Nova_Acao()
    {
        var booking = CriarBookingComStatus(Status.Paid);
        booking.ChangeState(Action.Finish);

        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Pay));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Cancel));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Refound));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.ReOpen));

        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Apos_Refunded_Deve_Lancar_Excecao_Para_Qualquer_Nova_Acao()
    {
        var booking = CriarBookingComStatus(Status.Paid);
        booking.ChangeState(Action.Refound);

        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Pay));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Cancel));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Finish));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.ReOpen));

        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Quando_Cancelado_E_Reaberto_Deveria_Permitir_Pagamento_Novamente()
    {
        var booking = CriarBookingComStatus(Status.Created);

        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.ReOpen);
        booking.ChangeState(Action.Pay);

        Assert.Equal(Status.Paid, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Quando_Acao_Invalida_For_Chamada_Mais_De_Uma_Vez_Deve_Lancar_Excecao_E_Manter_O_Status()
    {
        var booking = CriarBookingComStatus(Status.Created);

        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Finish));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Finish));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Refound));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.ReOpen));

        Assert.Equal(Status.Created, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Sequencia_Mista_De_Acoes_Invalidas_E_Validas_Deve_Respeitar_A_Regra()
    {
        var booking = CriarBookingComStatus(Status.Created);

        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Finish));
        booking.ChangeState(Action.Pay);
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.ReOpen));
        booking.ChangeState(Action.Finish);
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Cancel));

        Assert.Equal(Status.Finished, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Deve_Manter_Consistencia_Em_Varias_Transicoes()
    {
        var booking = CriarBookingComStatus(Status.Created);

        booking.ChangeState(Action.Cancel);
        booking.ChangeState(Action.ReOpen);
        booking.ChangeState(Action.Pay);
        booking.ChangeState(Action.Refound);
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.ReOpen));
        Assert.Throws<InvalidBookingStateTransitionException>(() => booking.ChangeState(Action.Pay));

        Assert.Equal(Status.Refunded, booking.CurrentStatus);
    }

    [Fact]
    public void ChangeState_Nao_Deve_Afetar_Outras_Propriedades_Do_Objeto()
    {
        var booking = CriarBookingComStatus(Status.Created);
        booking.Id = 10;
        booking.PlacedAt = new DateTime(2026, 03, 09, 10, 00, 00);
        booking.Start = new DateTime(2026, 03, 10, 14, 00, 00);
        booking.End = new DateTime(2026, 03, 10, 16, 00, 00);
        booking.RoomId = 2;
        booking.GuestId = 3;

        booking.ChangeState(Action.Pay);

        Assert.Equal(10, booking.Id);
        Assert.Equal(new DateTime(2026, 03, 09, 10, 00, 00), booking.PlacedAt);
        Assert.Equal(new DateTime(2026, 03, 10, 14, 00, 00), booking.Start);
        Assert.Equal(new DateTime(2026, 03, 10, 16, 00, 00), booking.End);
        Assert.Equal(2, booking.RoomId);
        Assert.Equal(3, booking.GuestId);
        Assert.Equal(Status.Paid, booking.CurrentStatus);
    }

    private static Booking CriarBookingComStatus(Status status)
    {
        var booking = new Booking();
        booking.SetStatus(status);
        return booking;
    }
}