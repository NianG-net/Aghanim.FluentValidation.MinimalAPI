using FluentValidation;
using Validation.lib;

namespace Validation.test;



public class AddValidatorsTest
{
    [Fact]
    public void AddValidatorsFromAssemblyContaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<WeatherRequestValidator>();
        
        // Assert
        Assert.Equal(2, services.Count);

        // Act
        var serviceProvider = services.BuildServiceProvider();

       
        // Assert
        var validator1 = serviceProvider.GetService<IValidator<TestRequest>>();

        Assert.NotNull(validator1);

        var validator2 = serviceProvider.GetService<IValidator<WeatherRequest>>();

        Assert.NotNull(validator2);

    }
}
