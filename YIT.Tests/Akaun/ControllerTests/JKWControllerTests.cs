using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT.Akaun.Controllers._01Jadual;

namespace YIT.Tests.Akaun.ControllerTests
{
    public class JKWControllerTests
    {
        //private JKWController _jkwController;
        //private _IUnitOfWork _unitOfWork;
        //private UserManager<IdentityUser> _userManager;
        //private _AppLogIRepository<AppLog, int> _appLog;
        //private CartJKW _cart;
        //private _IApplicationUserRepository _appUser;

        //public JKWControllerTests()
        //{
        //    // dependencies
        //    //_unitOfWork = A.Fake<_IUnitOfWork>();
        //    //_userManager = A.Fake<UserManager<IdentityUser>>();
        //    //_appLog = A.Fake<_AppLogIRepository<AppLog, int>>();
        //    //_cart = A.Fake<CartJKW>();
        //    //_appUser = A.Fake<_IApplicationUserRepository>();

        //    //// SUT
        //    //_jkwController = new JKWController(_unitOfWork, _userManager, _appLog, _cart, _appUser);
        //}

        //[Fact]
        //public void JKWController_Index_ReturnSuccess()
        //{
        //    // Arrange - what do I need to bring in
        //    var jkws = A.Fake<IEnumerable<JKW>>();
        //    A.CallTo(() => _unitOfWork.JKWRepo.GetAll()).Returns(jkws);

        //    // Act
        //    var result = _jkwController.Index();

        //    // Assert - Object check actions
        //    result.Should().BeOfType<ViewResult>();
        //}

        //[Fact]
        //public void JKWController_Details_ReturnSuccess()
        //{
        //    // Arrange
        //    var id = 1;
        //    var jkw = A.Fake<JKW>();
        //    A.CallTo(() => _unitOfWork.JKWRepo.GetAllDetailsById(id)).Returns(jkw);

        //    // Act
        //    var result = _jkwController.Details(id);

        //    // Assert
        //    result.Should().BeOfType<ViewResult>();
        //}

    }
}
