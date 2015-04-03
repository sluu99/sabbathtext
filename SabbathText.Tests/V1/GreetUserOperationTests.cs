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
        [TestMethod]
        public void Run_ShouldReturnAccepted()
        {
            GreetUserOperation operation = new GreetUserOperation(this.CreateContext());
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
