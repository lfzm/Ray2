using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class PostgreSqlEventStorageTests
    {
        private PostgreSqlEventStorage storage;
        private string TableName;
        public PostgreSqlEventStorageTests()
        {
            this.TableName = "es_storage";
            var sp = FakeConfig.BuildServiceProvider();
            //storage = new PostgreSqlEventStorage(sp, FakeConfig.ProviderName);
        }

        [Fact]
        public void should_Save_Success()
        {

        }

        [Fact]
        public void should_SQLSave_SingleSuccess()
        {

        }

        [Fact]
        public void should_BinarySave_SingleSuccess()
        {

        }

        [Fact]
        public void should_SQLSave_BatchSuccess()
        {

        }

        [Fact]
        public void should_BinarySave_BatchSuccess()
        {

        }


        [Fact]
        public void should_SQLSave_Unique()
        {

        }

        [Fact]
        public void should_BinarySave_Unique()
        {

        }

        [Fact]
        public void should_SQLSave_UniqueIndex()
        {

        }

        [Fact]
        public void should_BinarySave_UniqueIndex()
        {

        }


        [Fact]
        public void should_SQLSave_BatchException()
        {

        }

        [Fact]
        public void should_BinarySave_BatchException()
        {

        }



    }

}
