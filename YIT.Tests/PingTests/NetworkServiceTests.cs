﻿using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using YIT._DataAccess.Services.DNS;
using YIT._DataAccess.Services.Ping;

namespace YIT.Tests.PingTests
{
    public class NetworkServiceTests
    {
        private readonly NetworkService _pingService;
        private readonly IDNS _dNS;
        public NetworkServiceTests()
        {

            // Dependencies
            _dNS = A.Fake<IDNS>();

            //SUT - system under test
            _pingService = new NetworkService(_dNS);
        }
        // ClassName_Method_ExpectedReturn
        [Fact]
        public void NetworkService_SendPing_ReturnString()
        {
            // Arrange = variables, classes, mocks (What should i need to bring in?)
            A.CallTo(() => _dNS.SendDNS()).Returns(true);

            // Act = Executes
            var result = _pingService.SendPing();

            // Assert - expected results
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Be("Success: Ping Sent!");
            result.Should().Contain("Success", Exactly.Once());
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 2, 4)]
        public void NetworkService_PingTimeOut_ReturnInt(int a, int b, int expected)
        {
            // Arrange 

            // Act
            var result = _pingService.PingTimeout(a, b);

            // Assert
            result.Should().Be(expected);
            result.Should().BeGreaterThanOrEqualTo(2);
            result.Should().NotBeInRange(-10000, 0);
        }

        [Fact]
        public void NetworkService_LastPingDate_ReturnDate()
        {
            // Arrange = variables, classes, mocks

            // Act = Executes
            var result = _pingService.LastPingDate();

            // Assert - expected results
            result.Should().BeAfter(1.January(2024));
            result.Should().BeBefore(1.January(2025));
        }

        [Fact]
        public void NetworkService_GetPingOptions_ReturnsObject()
        {
            // Arrange
            var expected = new PingOptions()
            {
                DontFragment = true,
                Ttl = 1
            };

            // Act
            var result = _pingService.GetPingOptions();

            // Assert WARNING: "Be" careful
            result.Should().BeOfType<PingOptions>();
            result.Should().BeEquivalentTo(expected);
            result.Ttl.Should().Be(1);
        }

        [Fact]
        public void NetworkService_MostRecentPings_ReturnsObject()
        {
            // Arrange
            var expected = new PingOptions()
            {
                DontFragment = true,
                Ttl = 1
            };

            // Act
            var result = _pingService.MostRecentPings();

            // Assert WARNING: "Be" careful
            //result.Should().BeOfType<IEnumerable<PingOptions[]>>();
            result.Should().ContainEquivalentOf(expected);
            result.Should().Contain(x => x.DontFragment == true);
        }
    }
}
