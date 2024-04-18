using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
    public class GenericRepositoryTests
    {
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            if (await context.JKW.CountAsync() <= 0)
            {
                context.JKW.Add(
                new JKW()
                {
                    Kod = "1",
                    Perihal = "YAYASAN ISLAM TERENGGANU"
                });

                await context.SaveChangesAsync();

            }

            return context;
        }

        [Fact]
        public async void GenericRepository_IsExist_ByKod_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            string kod = "1";
            var jkwRepo = new JKWRepository(dbContext);

            // Act
            var result = jkwRepo.IsExist(jkw => jkw.Kod == kod);

            // Assert
            result.Should().BeTrue();
        }
    }
}
