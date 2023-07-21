using System.Security.Claims;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Helper;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;
public class UserHelperTests
{
    private readonly UserHelper _userHelper;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;
    private readonly Mock<ICreateEstablishmentCommand> _createEstablishmentCommandMock;

    public UserHelperTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _planTechDbContextMock = new Mock<IPlanTechDbContext>();
        _createEstablishmentCommandMock = new Mock<ICreateEstablishmentCommand>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("organisation", "{\n  \"ukprn\" : \"131\",\n  \"type\" : {\n      \"name\" : \"Type name\"\n  },\n  \"name\" : \"Organisation name\"\n}"),
            }, "mock"));

        _httpContextAccessorMock.Setup(m => m.HttpContext).Returns(new DefaultHttpContext() { User = user });

        _userHelper = new UserHelper(_httpContextAccessorMock.Object, _planTechDbContextMock.Object, _createEstablishmentCommandMock.Object);
    }

    [Fact]
    public async Task GetCurrentUserId_Returns_Correct_Id_When_UserExists_InDatabase()
    {
        _planTechDbContextMock.Setup(m => m.GetUserBy(userModel => userModel.DfeSignInRef == "1")).ReturnsAsync(new User() { Id = 1 });

        var result = await _userHelper.GetCurrentUserId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetEstablishmentId_Returns_Correct_Id_When_Establishment_Exists_In_DB()
    {
        _planTechDbContextMock.Setup(m => m.GetEstablishmentBy(establishment => establishment.EstablishmentRef == "131")).ReturnsAsync(new Establishment() { Id = 1 });

        var result = await _userHelper.GetEstablishmentId();

        Assert.IsType<int>(result);

        Assert.Equal(1, result);
    }


    [Fact]
    public async Task GetEstablishmentId_Returns_Correct_Id_When_Establishment_Does_Not_Exists_In_DB()
    {
        _planTechDbContextMock
            .SetupSequence(m => m.GetEstablishmentBy(establishment => establishment.EstablishmentRef == "131"))
            .ReturnsAsync(() => { return null; })
            .ReturnsAsync(new Establishment() { Id = 17 });
        
        var result = await _userHelper.GetEstablishmentId();

        Assert.IsType<int>(result);

        Assert.Equal(17, result);
    } 
}