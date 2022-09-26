using NerdStore.Core.DomainObjects;

namespace NerdStore.Vendas.Domain;

public class Pedido
{
    public static int MAX_UNIDADES_ITEM => 15;
    public static int MIN_UNIDADES_ITEM => 1;

    protected Pedido()
    {
        _pedidoItems = new List<PedidoItem>();
    }

    public Guid ClienteId { get; set; }
    public decimal ValorTotal { get; private set; }
    public PedidoStatus PedidoStatus { get; private set; }

    private readonly List<PedidoItem> _pedidoItems;
    public IReadOnlyCollection<PedidoItem> PedidoItems => _pedidoItems.AsReadOnly();


    private void CalcularValorPedido()
        => ValorTotal = PedidoItems.Sum(i => i.CalcularValor());

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



public enum PedidoStatus
{
    Rascunho = 0,
    Iniciado = 1,
    Pago = 4,
    Entregue = 5,
    Cancelado = 6
}



public class PedidoItem
{
    public Guid ProdutoId { get; private set; }
    public string ProdutoNome { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }

    public PedidoItem(Guid produtoId, string produtoNome, int quantidade, decimal valorUnitario)
    {
        if (quantidade < Pedido.MIN_UNIDADES_ITEM)
        {
            throw new DomainException($"Mínimo de {Pedido.MIN_UNIDADES_ITEM} unidades por produto");
        }

        ProdutoId = produtoId;
        ProdutoNome = produtoNome;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    internal void AdicionarUnidades(int unidades)
        => Quantidade += unidades;

    internal decimal CalcularValor()
        => Quantidade * ValorUnitario;
}