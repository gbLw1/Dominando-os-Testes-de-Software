using FluentValidation.Results;
using NerdStore.Core.DomainObjects;

namespace NerdStore.Vendas.Domain;

public class Pedido : Entity
{
    public static int MAX_UNIDADES_ITEM => 15;
    public static int MIN_UNIDADES_ITEM => 1;

    protected Pedido()
    {
        _pedidoItems = new List<PedidoItem>();
    }

    public Guid ClienteId { get; private set; }
    public decimal ValorTotal { get; private set; }
    public decimal Desconto { get; private set; }
    public PedidoStatus PedidoStatus { get; private set; }
    public bool VoucherUtilizado { get; private set; }
    public Voucher Voucher { get; private set; }

    private readonly List<PedidoItem> _pedidoItems;
    public IReadOnlyCollection<PedidoItem> PedidoItems => _pedidoItems.AsReadOnly();

    public ValidationResult AplicarVoucher(Voucher voucher)
    {
        var result = voucher.ValidarSeAplicavel();
        if (result.IsValid is false)
        {
            return result;
        }

        Voucher = voucher;
        VoucherUtilizado = true;
        CalcularValorTotalDesconto();

        return result;
    }

    public void CalcularValorTotalDesconto()
    {
        if (VoucherUtilizado is false)
        {
            return;
        }

        decimal desconto = 0;
        var valor = ValorTotal;

        if (Voucher.TipoDescontoVoucher == TipoDescontoVoucher.Valor)
        {
            if (Voucher.ValorDesconto.HasValue)
            {
                desconto = Voucher.ValorDesconto.Value;
                valor -= desconto;
            }
        }
        else
        {
            if (Voucher.PercentualDesconto.HasValue)
            {
                desconto = (ValorTotal * Voucher.PercentualDesconto.Value) / 100;
                valor -= desconto;
            }
        }

        ValorTotal = valor < 0 ? 0 : valor;
        Desconto = desconto;
    }

    private void CalcularValorPedido()
    {
        ValorTotal = PedidoItems.Sum(i => i.CalcularValor());
        CalcularValorTotalDesconto();
    }

    private bool PedidoExistente(PedidoItem item)
        => _pedidoItems.Any(p => p.ProdutoId == item.ProdutoId);

    private void ValidarPedidoItemInexistente(PedidoItem item)
    {
        if (PedidoExistente(item) is false)
        {
            throw new DomainException("O item não existe no pedido");
        }
    }

    private void ValidarQuantidadeItemPermitida(PedidoItem item)
    {
        var quantidadeItens = item.Quantidade;
        if (PedidoExistente(item))
        {
            var itemExistente = _pedidoItems.FirstOrDefault(p => p.ProdutoId == item.ProdutoId);
            quantidadeItens += itemExistente.Quantidade;
        }

        if (quantidadeItens > MAX_UNIDADES_ITEM)
        {
            throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades por produto");
        }
    }

    public void AdicionarItem(PedidoItem item)
    {
        ValidarQuantidadeItemPermitida(item);

        if (PedidoExistente(item))
        {
            var itemExistente = _pedidoItems.FirstOrDefault(p => p.ProdutoId == item.ProdutoId);

            itemExistente.AdicionarUnidades(item.Quantidade);
            item = itemExistente;

            _pedidoItems.Remove(itemExistente);
        }

        _pedidoItems.Add(item);
        CalcularValorPedido();
    }

    public void AtualizarItem(PedidoItem item)
    {
        ValidarPedidoItemInexistente(item);
        ValidarQuantidadeItemPermitida(item);

        var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == item.ProdutoId);

        _pedidoItems.Remove(itemExistente);
        _pedidoItems.Add(item);

        CalcularValorPedido();
    }

    public void RemoverItem(PedidoItem item)
    {
        ValidarPedidoItemInexistente(item);

        _pedidoItems.Remove(item);

        CalcularValorPedido();
    }

    public void TornarRascunho()
        => PedidoStatus = PedidoStatus.Rascunho;

    public static class PedidoFactory
    {
        public static Pedido NovoPedidoRascunho(Guid clienteId)
        {
            var pedido = new Pedido
            {
                ClienteId = clienteId,
            };

            pedido.TornarRascunho();
            return pedido;
        }
    }
}