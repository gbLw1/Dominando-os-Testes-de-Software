namespace NerdStore.Vendas.Domain.Tests;

public class VoucherTests
{
    [Fact(DisplayName = "Validar Voucher Tipo Valor Válido")]
    [Trait("Categoria", "Vendas - Voucher")]
    public void Voucher_ValidarVoucherTipoValor_DeveEstarValido()
    {
        // Arrange
        var voucher = new Voucher(codigo: "PROMO-15-REAIS",
            percentualDesconto: null,
            valorDesconto: 15,
            quantidade: 1,
            tipoDescontoVoucher: TipoDescontoVoucher.Valor,
            dataValidade: DateTime.Now.AddDays(15),
            ativo: true,
            utilizado: false);

        // Act
        var result = voucher.ValidarSeAplicavel();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validar Voucher Tipo Valor Inválido")]
    [Trait("Categoria", "Vendas - Voucher")]
    public void Voucher_ValidarVoucherTipoValor_DeveEstarInvalido()
    {
        // Arrange
        var voucher = new Voucher(codigo: "",
            percentualDesconto: null,
            valorDesconto: null,
            quantidade: 0,
            tipoDescontoVoucher: TipoDescontoVoucher.Valor,
            dataValidade: DateTime.Now.AddDays(-1),
            ativo: false,
            utilizado: true);

        // Act
        var result = voucher.ValidarSeAplicavel();

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(6, result.Errors.Count);
        Assert.Contains(VoucherAplicavelValidation.AtivoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.CodigoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.DataValidadeErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.QuantidadeErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.UtilizadoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.ValorDescontoErroMsg, result.Errors.Select(c => c.ErrorMessage));
    }

    [Fact(DisplayName = "Validar Voucher Porcentagem Válido")]
    [Trait("Categoria", "Vendas - Voucher")]
    public void Voucher_ValidarVoucherPorcentagem_DeveEstarValido()
    {
        var voucher = new Voucher("PROMO-15-OFF", 15, null, 1,
            TipoDescontoVoucher.Porcentagem, DateTime.Now.AddDays(15), true, false);

        // Act
        var result = voucher.ValidarSeAplicavel();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validar Voucher Porcentagem Inválido")]
    [Trait("Categoria", "Vendas - Voucher")]
    public void Voucher_ValidarVoucherPorcentagem_DeveEstarInvalido()
    {
        var voucher = new Voucher("", null, null, 0,
            TipoDescontoVoucher.Porcentagem, DateTime.Now.AddDays(-1), false, true);

        // Act
        var result = voucher.ValidarSeAplicavel();

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(6, result.Errors.Count);
        Assert.Contains(VoucherAplicavelValidation.AtivoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.CodigoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.DataValidadeErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.QuantidadeErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.UtilizadoErroMsg, result.Errors.Select(c => c.ErrorMessage));
        Assert.Contains(VoucherAplicavelValidation.PercentualDescontoErroMsg, result.Errors.Select(c => c.ErrorMessage));
    }
}
