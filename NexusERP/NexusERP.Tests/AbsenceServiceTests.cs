using Moq;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Tests
{
    public class AbsenceServiceTests
    {
        [Fact]
        public async Task SubmitRequest_EndDateBeforeStartDate_ThrowsAppException()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var invalidAbsence = new UserAbsence
            {
                StartDate = new DateTime(2026, 7, 10),
                EndDate = new DateTime(2026, 7, 5) 
            };

            var ex = await Assert.ThrowsAsync<AppException>(() => service.SubmitRequestAsync(1, invalidAbsence));
            Assert.Contains("before start date", ex.Message);
            mockRepo.Verify(r => r.SubmitRequestAsync(It.IsAny<UserAbsence>()), Times.Never);
        }

        [Fact]
        public async Task SubmitRequest_OverlappingDates_ThrowsAppException()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var absence = new UserAbsence
            {
                StartDate = new DateTime(2026, 7, 1),
                EndDate = new DateTime(2026, 7, 5)
            };

            mockRepo.Setup(r => r.HasOverlappingAbsenceAsync(1, absence.StartDate, absence.EndDate))
                    .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<AppException>(() => service.SubmitRequestAsync(1, absence));
            Assert.Contains("already have an active or pending", ex.Message);
            mockRepo.Verify(r => r.SubmitRequestAsync(It.IsAny<UserAbsence>()), Times.Never);
        }

        [Fact]
        public async Task SubmitRequest_ValidRequest_SetsPendingAndSaves()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var absence = new UserAbsence
            {
                StartDate = new DateTime(2026, 7, 1),
                EndDate = new DateTime(2026, 7, 5)
            };

            mockRepo.Setup(r => r.HasOverlappingAbsenceAsync(1, absence.StartDate, absence.EndDate))
                    .ReturnsAsync(false);

            await service.SubmitRequestAsync(99, absence);

            Assert.Equal(99, absence.UserId);
            Assert.Equal(AbsenceStatus.Pending, absence.Status);
            mockRepo.Verify(r => r.SubmitRequestAsync(absence), Times.Once);
        }
    }
}
