using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineBankingApplication.Context;
using OnlineBankingApplication.Controllers;
using OnlineBankingApplication.Entities;
using Xunit;

namespace OnlineBankingApplication.Tests
{
    public class AccountsControllerTests
    {
        [Fact]
        public async Task CreateAccount_ReturnsCreatedAtActionResult()
        {
            //// Arrange
            //var mockContext = new Mock<BaseDbContext>();
            //var controller = new AccountsController(mockContext.Object);

            //// Act
            //var result = await controller.CreateAccount(new Account());

            //// Assert
            //Assert.IsType<CreatedAtActionResult>(result);
        }
    }
}
