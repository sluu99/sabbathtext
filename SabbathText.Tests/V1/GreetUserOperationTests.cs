namespace SabbathText.Tests.V1
{
    using System;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.V1;

    /// <summary>
    /// Tests GreetUserOperation
    /// </summary>
    [TestClass]
    public class GreetUserOperationTests : TestBase
    {
        /// <summary>
        /// This test makes sure GreetUserOperation will return HTTP Accepted
        /// on successful cases.
        /// </summary>
        [TestMethod]
        public void GreetUserOperation_ShouldReturnAccepted()
        {
            GreetUserOperation operation = new GreetUserOperation(this.CreateContext());
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        }

        /// <summary>
        /// This test makes sure that the GreetUserOperation is pushed to completion,
        /// and will send out messages
        /// </summary>
        [TestMethod]
        public void GreetUserOperation_ShouldFinishSendingTheMessage()
        {
            OperationContext context = this.CreateContext();
            GreetUserOperation operation = new GreetUserOperation(context);
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

            this.AssertOperationFinishes(context);
        }
    }
}
