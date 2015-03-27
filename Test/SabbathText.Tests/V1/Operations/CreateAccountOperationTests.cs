namespace SabbathText.Tests.V1.Operations
{
    using System;
    using System.Net;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Compensation.V1;
    using SabbathText.V1.Entities;
    using SabbathText.V1.Operations;

    /// <summary>
    /// Tests CreateAccountOperation
    /// </summary>
    [TestClass]
    public class CreateAccountOperationTests
    {
        private OperationContext context;
        private CreateAccountOperation operation;

        /// <summary>
        /// This method is called before every test runs
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            this.context = new OperationContext
            {
                AccountStore = TestGlobals.AccountStore,
                CancellationToken = new CancellationTokenSource(TestGlobals.Settings.OperationTimeout).Token,
                IdentityStore = TestGlobals.IdentityStore,
                TrackingId = Guid.NewGuid().ToString(),
                Compensation = new CompensationClient(
                    TestGlobals.CheckpointStore, TestGlobals.CheckpointQueue, TestGlobals.Settings.CheckpointVisibilityTimeout),
            };

            this.operation = new CreateAccountOperation(this.context);
        }

        /// <summary>
        /// This test makes sure that the operation responses with a BadRequestCode with the phone number is invalid
        /// </summary>
        [TestMethod]
        public void CreateAccountOperation_ShouldReturnBadRequestOnInvalidPhoneNumber()
        {
            OperationResponse<Account> response = this.operation.CreateWithPhoneNumber(Guid.NewGuid().ToString()).Result;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Tests a successful case of CreateAccountOperation
        /// </summary>
        [TestMethod]
        public void CreateAccountOperation_Success()
        {
            string phoneNumber = TestHelper.GetUSPhoneNumber();
            OperationResponse<Account> response = this.operation.CreateWithPhoneNumber(phoneNumber).Result;
            
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(response.Data.PhoneNumber, phoneNumber);
        }
    }
}
