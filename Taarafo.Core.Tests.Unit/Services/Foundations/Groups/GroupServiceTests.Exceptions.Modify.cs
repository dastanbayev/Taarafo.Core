// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Taarafo.Core.Models.Groups;
using Taarafo.Core.Models.Groups.Exceptions;
using Xunit;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.Groups
{
	public partial class GroupServiceTests
	{
		[Fact]
		public async Task ShouldThrowCriticalDependencyExceptionOnUpdateIfSqlErrorOccursAndLogItAsync()
		{
			// given
			Group randomGroup = CreateRandomGroup();
			SqlException sqlException = GetSqlException();

			var failedGroupStorageException =
				new FailedGroupStorageException(sqlException);

			var expectedGroupDependencyException =
				new GroupDependencyException(failedGroupStorageException);

			this.dateTimeBrokerMock.Setup(broker =>
				broker.GetCurrentDateTimeOffset())
					.Throws(sqlException);

			// when
			ValueTask<Group> modifyGroupTask =
				this.groupService.UpdateGroupAsync(randomGroup);

			// then
			await Assert.ThrowsAsync<GroupDependencyException>(() =>
			   modifyGroupTask.AsTask());

			this.dateTimeBrokerMock.Verify(broker =>
				broker.GetCurrentDateTimeOffset(),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.SelectGroupByIdAsync(randomGroup.Id),
					Times.Never);

			this.storageBrokerMock.Verify(broker =>
				broker.UpdateGroupAsync(randomGroup),
					Times.Never);

			this.loggingBrokerMock.Verify(broker =>
				broker.LogCritical(It.Is(SameExceptionAs(
					expectedGroupDependencyException))),
						Times.Once);

			this.dateTimeBrokerMock.VerifyNoOtherCalls();
			this.storageBrokerMock.VerifyNoOtherCalls();
			this.loggingBrokerMock.VerifyNoOtherCalls();
		}

		[Fact]
		public async void ShouldThrowValidationExceptionOnUpdateIfReferenceErrorOccursAndLogItAsync()
		{
			// given
			Group someGroup =
				CreateRandomGroup();

			Group foreignKeyConflictedGroup =
				someGroup;

			string randomMessage =
				GetRandomMessage();

			string exceptionMessage =
				randomMessage;

			var foreignKeyConstraintConflictException =
				new ForeignKeyConstraintConflictException(exceptionMessage);

			var invalidGroupReferenceException =
				new InvalidGroupReferenceException(foreignKeyConstraintConflictException);

			var groupDependencyValidationException =
				new GroupDependencyException(invalidGroupReferenceException);

			this.dateTimeBrokerMock.Setup(broker =>
				broker.GetCurrentDateTimeOffset())
					.Throws(foreignKeyConstraintConflictException);

			// when
			ValueTask<Group> modifyGroupTask =
				this.groupService.UpdateGroupAsync(foreignKeyConflictedGroup);

			// then
			await Assert.ThrowsAsync<GroupDependencyException>(() =>
				modifyGroupTask.AsTask());

			this.dateTimeBrokerMock.Verify(broker =>
				broker.GetCurrentDateTimeOffset(),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.SelectGroupByIdAsync(foreignKeyConflictedGroup.Id),
					Times.Never);

			this.loggingBrokerMock.Verify(broker =>
				broker.LogError(It.Is(SameExceptionAs(groupDependencyValidationException))),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.UpdateGroupAsync(foreignKeyConflictedGroup),
					Times.Never);

			this.dateTimeBrokerMock.VerifyNoOtherCalls();
			this.storageBrokerMock.VerifyNoOtherCalls();
			this.loggingBrokerMock.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
		{
			// given
			Group randomGroup = CreateRandomGroup();
			var databaseUpdateException = new DbUpdateException();

			var failedGroupStorageException =
				new FailedGroupStorageException(databaseUpdateException);

			var expectedGroupDependencyException =
				new GroupDependencyException(failedGroupStorageException);

			this.dateTimeBrokerMock.Setup(broker =>
				broker.GetCurrentDateTimeOffset())
					.Throws(databaseUpdateException);

			// when
			ValueTask<Group> modifyGroupTask =
				this.groupService.UpdateGroupAsync(randomGroup);

			// then
			await Assert.ThrowsAsync<GroupDependencyException>(() =>
				modifyGroupTask.AsTask());

			this.dateTimeBrokerMock.Verify(broker =>
				broker.GetCurrentDateTimeOffset(),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.SelectGroupByIdAsync(randomGroup.Id),
					Times.Never);

			this.loggingBrokerMock.Verify(broker =>
				broker.LogError(It.Is(SameExceptionAs(
					expectedGroupDependencyException))),
						Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.UpdateGroupAsync(randomGroup),
					Times.Never);

			this.dateTimeBrokerMock.VerifyNoOtherCalls();
			this.storageBrokerMock.VerifyNoOtherCalls();
			this.loggingBrokerMock.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task ShouldThrowDependencyValidationExceptionOnUpdateIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
		{
			// given
			Group randomGroup = CreateRandomGroup();
			var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

			var lockedGroupException =
				new LockedGroupException(databaseUpdateConcurrencyException);

			var expectedGroupDependencyValidationException =
				new GroupDependencyException(lockedGroupException);

			this.dateTimeBrokerMock.Setup(broker =>
				broker.GetCurrentDateTimeOffset())
					.Throws(databaseUpdateConcurrencyException);

			// when
			ValueTask<Group> modifyGroupTask =
				this.groupService.UpdateGroupAsync(randomGroup);

			// then
			await Assert.ThrowsAsync<GroupDependencyException>(() =>
				modifyGroupTask.AsTask());

			this.dateTimeBrokerMock.Verify(broker =>
				broker.GetCurrentDateTimeOffset(),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.SelectGroupByIdAsync(randomGroup.Id),
					Times.Never);

			this.loggingBrokerMock.Verify(broker =>
				broker.LogError(It.Is(SameExceptionAs(
					expectedGroupDependencyValidationException))),
						Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.UpdateGroupAsync(randomGroup),
					Times.Never);

			this.dateTimeBrokerMock.VerifyNoOtherCalls();
			this.storageBrokerMock.VerifyNoOtherCalls();
			this.loggingBrokerMock.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task ShouldThrowServiceExceptionOnUpdateIfServiceErrorOccursAndLogItAsync()
		{
			// given
			Group randomGroup = CreateRandomGroup();
			var serviceException = new Exception();

			var failedGroupException =
				new FailedGroupServiceException(serviceException);

			var expectedGroupServiceException =
				new GroupServiceException(failedGroupException);

			this.dateTimeBrokerMock.Setup(broker =>
				broker.GetCurrentDateTimeOffset())
					.Throws(serviceException);

			// when
			ValueTask<Group> modifyGroupTask =
				this.groupService.UpdateGroupAsync(randomGroup);

			// then
			await Assert.ThrowsAsync<GroupServiceException>(() =>
				modifyGroupTask.AsTask());

			this.dateTimeBrokerMock.Verify(broker =>
				broker.GetCurrentDateTimeOffset(),
					Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.SelectGroupByIdAsync(randomGroup.Id),
					Times.Never);

			this.loggingBrokerMock.Verify(broker =>
				broker.LogError(It.Is(SameExceptionAs(
					expectedGroupServiceException))),
						Times.Once);

			this.storageBrokerMock.Verify(broker =>
				broker.UpdateGroupAsync(randomGroup),
					Times.Never);

			this.dateTimeBrokerMock.VerifyNoOtherCalls();
			this.storageBrokerMock.VerifyNoOtherCalls();
			this.loggingBrokerMock.VerifyNoOtherCalls();
		}
	}
}
