using Microsoft.VisualStudio.TestTools.UnitTesting;
using blazorTest.Server.Services;
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework.Internal;
using NUnit.Framework;
using blazorTest.ServerTests.ContextBase;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using System.Linq;

namespace blazorTest.Server.Services.Tests
{
    [TestClass()]
    public class PostServiceTests
    {
        public PostServiceBase Fixture { get; set; }

        public PostServiceTests()
        {
            Fixture = new PostServiceBase();
        }

        [TestMethod()]
        public void ReadPostWhenWindowOpenedTest()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    var roomId = context.Rooms
                        .Where(room => room.Name == "room1")
                        .Single()
                        .Id;

                    var service = new PostService(context);

                    var result = service.ReadPostWhenWindowOpened(roomId, "");

                    Assert.AreEqual("data1", result[0].Text);
                }
            }
        }
    }
}