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




        [Fact]
        public async Task ReviewRequest_AbsenceNotFound_ThrowsAppException()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserAbsence?)null);

            var ex = await Assert.ThrowsAsync<AppException>(() => service.ReviewRequestAsync(1, 99, "Approved", null));
            Assert.Contains("Leave request not found", ex.Message);
        }

        [Fact]
        public async Task ReviewRequest_AlreadyProcessed_ThrowsAppException()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var processedAbsence = new UserAbsence { Status = AbsenceStatus.Approved };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(processedAbsence);

            var ex = await Assert.ThrowsAsync<AppException>(() => service.ReviewRequestAsync(1, 99, "Rejected", null));
            Assert.Contains("already been processed", ex.Message);
        }

        [Theory]
        [InlineData("Pending")]
        [InlineData("GarbageStatus")]
        public async Task ReviewRequest_InvalidStatusString_ThrowsAppException(string invalidStatus)
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var pendingAbsence = new UserAbsence { Status = AbsenceStatus.Pending };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pendingAbsence);

            var ex = await Assert.ThrowsAsync<AppException>(() => service.ReviewRequestAsync(1, 99, invalidStatus, null));
            Assert.Contains("Invalid review status", ex.Message);
        }

        [Fact]
        public async Task ReviewRequest_ValidApproval_UpdatesAndSaves()
        {
            var mockRepo = new Mock<IAbsenceRepository>();
            var service = new AbsenceService(mockRepo.Object);

            var pendingAbsence = new UserAbsence { Status = AbsenceStatus.Pending };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pendingAbsence);

            await service.ReviewRequestAsync(1, 42, "Approved", "Enjoy your vacation");

            Assert.Equal(AbsenceStatus.Approved, pendingAbsence.Status);
            Assert.Equal(42, pendingAbsence.ReviewedByUserId);
            Assert.Equal("Enjoy your vacation", pendingAbsence.ReviewerComments);
            mockRepo.Verify(r => r.ReviewRequestAsync(pendingAbsence), Times.Once);
        }
    }
}
