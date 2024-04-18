using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;

namespace YIT.Tests.DataAccess.Repositories
{
    public class JKWRepositoryTests
    {
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            if (await context.JKW.CountAsync() < 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    context.JKW.Add(
                    new JKW()
                    {
                        Kod = "1",
                        Perihal = "YAYASAN ISLAM TERENGGANU"
                    });

                    await context.SaveChangesAsync();
                }
                
            }

            return context;
        }

        [Fact]
        public async void JKWRepository_GetAllDetails_ReturnsSuccess()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var jkwRepo = new JKWRepository(dbContext);

            // Act
            var result = jkwRepo.GetAllDetails();

            // Assert
            result.Should().BeOfType<List<JKW>>();
        }

        [Fact]
        public async void JKWRepository_GetAllDetailsById_ReturnsJKW()
        {
            // Arrange
            var id = 1;
            var dbContext = await GetDbContext();
            var jkwRepo = new JKWRepository(dbContext);

            // Act
            var result = jkwRepo.GetAllDetailsById(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<JKW>();
        }
        
    }
}
