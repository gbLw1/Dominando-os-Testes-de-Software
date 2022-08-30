namespace Demo.Tests;

public class AssertNumberTests
{
    [Fact]
    public void Calculadora_Somar_DeveSerIgual()
    {
        // Arrange
        var calculadora = new Calculadora();

        // Act
        var resultado = calculadora.Somar(1, 2);

        // Assert
        Assert.Equal(3, resultado);
    }

    [Fact]
    public void Calculadora_Somar_NaoDeveSerIgual()
    {
        // Arrange
        var calculadora = new Calculadora();

        // Act
        var resultado = calculadora.Somar(1.13123123123, 2.2312313123);

        // Assert
        Assert.NotEqual(3, resultado, 1);
    }
}
